using Microsoft.Win32;
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using thecalcify.Helper;

namespace thecalcify.CustomActions
{
    [RunInstaller(true)]
    public class Installer1 : Installer
    {
        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
        private const int WM_CLOSE = 0x0010;

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            try
            {
                string targetDir = @"C:\Program Files\thecalcify\thecalcify";
                if (!string.IsNullOrEmpty(targetDir))
                {
                    string exePath = Path.Combine(targetDir, "thecalcify.exe");
                    //ApplicationLogger.Log($"Target directory  :- {targetDir}");

                    if (File.Exists(exePath))
                    {
                        //ApplicationLogger.Log("Starting the application with admin privileges: " + exePath);

                        var psi = new ProcessStartInfo
                        {
                            FileName = exePath,
                            Verb = "runas", // Request admin elevation
                            UseShellExecute = true
                        };

                        Process.Start(psi);

                        //ApplicationLogger.Log("Application started successfully.");
                    }
                    //else
                    //{
                    //    ApplicationLogger.Log("Executable not found at: " + exePath);
                    //}
                }
                //else
                //{
                //    ApplicationLogger.Log($"Target directory is null or empty. Dir :- {targetDir}");
                //}
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);

            ApplicationLogger.Log("Uninstalling the application and closing running instances.");

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
