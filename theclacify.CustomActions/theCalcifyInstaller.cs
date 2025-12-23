using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Web;
using thecalcify.Helper;

namespace thecalcify.CustomActions
{
    [RunInstaller(true)]
    public class theCalcifyInstaller : Installer
    {
        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        private const int WM_CLOSE = 0x0010;

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            try
            {
                string targetDir = Context.Parameters["targetdir"].TrimEnd('\\');
                if (Directory.Exists(targetDir))
                    GrantDirectoryAccess(targetDir);

                // 🔥 AUTO-DETECT thecalcifyRTW.exe (very important)
                string exePath =
                    Directory.GetFiles(targetDir, "thecalcifyRTW.exe", SearchOption.AllDirectories)
                    .FirstOrDefault();

                if (exePath == null)
                    throw new Exception("thecalcifyRTW.exe not found in target directory!");

                // Log what path was found
                ApplicationLogger.Log($"Installing RTW service using path: {exePath}");

                // Install services
                Process.Start("sc", $"create thecalcifyRTW binPath= \"{exePath}\" start= auto displayname= \"thecalcify RTW Service\"")?.WaitForExit();

                Process.Start("sc", $"description thecalcifyRTW \"Real-Time Watcher service for thecalcify\"")?.WaitForExit();

                // Start service
                Process.Start("sc", $"start thecalcifyRTW")?.WaitForExit();
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);

            try
            {
                Process.Start("sc", "stop thecalcifyRTW")?.WaitForExit();
                Process.Start("sc", "delete thecalcifyRTW")?.WaitForExit();
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }

            CloseRunningTheCalcify();
        }

        private void CloseRunningTheCalcify()
        {
            var processes = Process.GetProcessesByName("thecalcify");
            foreach (var proc in processes)
            {
                try
                {
                    PostMessage(proc.MainWindowHandle, WM_CLOSE, 0, 0);
                    proc.WaitForExit(5000);
                    if (!proc.HasExited)
                        proc.Kill();
                }
                catch (Exception ex)
                {
                    ApplicationLogger.LogException(ex);
                }
            }
        }

        public static void GrantDirectoryAccess(string fullPath)
        {
            try
            {
                DirectoryInfo dInfo = new DirectoryInfo(fullPath);
                DirectorySecurity dSecurity = dInfo.GetAccessControl();

                var accessRule = new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    FileSystemRights.FullControl,
                    InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow);

                dSecurity.AddAccessRule(accessRule);
                dInfo.SetAccessControl(dSecurity);
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }
    }
}
