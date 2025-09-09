using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Runtime.InteropServices;
using thecalcify.Helper;

namespace thecalcify.CustomActions
{
    [RunInstaller(true)]
    public class Installer1 : Installer
    {
        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        private const int WM_CLOSE = 0x0010;

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);

            var processes = Process.GetProcessesByName("thecalcify"); // Exclude .exe
            foreach (var proc in processes)
            {
                try
                {
                    PostMessage(proc.MainWindowHandle, WM_CLOSE, 0, 0);
                    proc.WaitForExit(5000);
                    if (!proc.HasExited)
                    {
                        proc.Kill();
                    }
                }
                catch (Exception ex)
                {
                    ApplicationLogger.LogException(ex);
                }
            }
        }
    }
}
