using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using thecalcify.Helper;

namespace thecalcify.Update_Service
{
    public static class PipeSignalReceiver
    {
        private static readonly string PipeName = "thecalcifyUpdatePipe";
        private static CancellationTokenSource _cts;
        private static Action<string> _onMessageReceived;
        private static int _updateTriggered = 0;

        public static void Start(Action<string> onMessageReceived)
        {
            _onMessageReceived = onMessageReceived;
            _cts = new CancellationTokenSource();

            Task.Run(async () => await Listen(_cts.Token));
            Task.Run(() => FileFound());
        }

        public static void Stop()
        {
            _cts?.Cancel();
        }

        private static async Task Listen(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using (var server = new NamedPipeServerStream(
                        PipeName,
                        PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances,
                        PipeTransmissionMode.Message,
                        PipeOptions.Asynchronous))
                    {
                        await server.WaitForConnectionAsync(token);

                        using (var reader = new StreamReader(server))
                        using (var writer = new StreamWriter(server) { AutoFlush = true })
                        {
                            string message = await reader.ReadLineAsync();

                            if (!string.IsNullOrEmpty(message))
                            {
                                TriggerUpdate("Service Signal");
                                _onMessageReceived?.Invoke(message);
                            }

                            writer.WriteLine("ACK");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    ApplicationLogger.LogException(ex);
                    await Task.Delay(1000);
                }
            }
        }

        public static void FileFound()
        {
            try
            {
                if (!File.Exists(APIUrl.InstallationPath))
                    return;

                string installedExe = Path.Combine(APIUrl.TempPath, "thecalcify", "thecalcify.exe");
                if (!File.Exists(installedExe))
                    return;

                DateTime installedModified = File.GetLastWriteTime(APIUrl.InstallationPath);
                DateTime installerModified = File.GetLastWriteTime(installedExe);

                if (installerModified > installedModified.AddMinutes(2))
                {
                    TriggerUpdate("Installer is newer");
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        private static void TriggerUpdate(string source)
        {
            if (Interlocked.Exchange(ref _updateTriggered, 1) == 1)
                return;

            if (Application.OpenForms.Count == 0)
                return;

            var form = Application.OpenForms[0];

            form.Invoke(new Action(() =>
            {
                var result = MessageBox.Show(
                    $"Update required.\nPlease restart the application to apply updates.",
                    "thecalcify Update",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    ApplicationLogger.Log($"Update triggered, uninstalling old version. by {source}");
                    UpdateAgent.UninstallOldVersion("thecalcify");
                }
                else
                {
                    ApplicationLogger.Log($"Update canceled by user. Source: {source}");
                    Stop();
                }
            }));
        }
    }
}
