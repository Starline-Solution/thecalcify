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
                RegisterRtdDll("thecalcifyRTD.dll");
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        public void RegisterRtdDll(string dllName, params string[] searchPaths)
        {
            try
            {
                // 🔹 Locate DLL
                string dllPath = searchPaths
                    .SelectMany(p => new[] { p, Path.Combine(p, dllName) })
                    .Concat(new[]
                    {
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dllName),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\bin\Debug", dllName),
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\bin\Release", dllName)
                    })
                    .Select(Path.GetFullPath)
                    .FirstOrDefault(File.Exists);

                if (dllPath == null)
                {
                    ApplicationLogger.Log($"RTD DLL '{dllName}' not found.");
                    return;
                }

                // 🔹 Pick RegAsm (Excel 32-bit → Framework, Excel 64-bit → Framework64)
                bool excel32 = true;
                try
                {
                    var excelApp = new Microsoft.Office.Interop.Excel.Application();
                    excel32 = !excelApp.OperatingSystem.Contains("64");
                    excelApp.Quit();
                }
                catch { excel32 = true; }

                string regasm = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                    excel32 ? @"Microsoft.NET\Framework\v4.0.30319\RegAsm.exe"
                            : @"Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe");

                // 🔹 Run unregister + register
                foreach (var args in new[] { $"/unregister \"{dllPath}\"", $"\"{dllPath}\" /codebase /tlb" })
                {
                    var psi = new ProcessStartInfo(regasm, args)
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    var proc = Process.Start(psi);
                    string output = proc.StandardOutput.ReadToEnd();
                    string error = proc.StandardError.ReadToEnd();
                    proc.WaitForExit();
                    if (proc.ExitCode != 0)
                        ApplicationLogger.Log($"RegAsm failed. Args: {args}\nOutput: {output}\nError: {error}");
                }

                ApplicationLogger.Log($"RTD DLL registered successfully: {dllPath}");

                // Set registry throttle interval based on Office version
                SetThrottle();
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log("RegisterRtdDll Error: " + ex.Message);
            }
        }


        public void SetThrottle()
        {
            try
            {
                string officeVersion = GetOfficeVersion();
                string registryPath = $@"Software\Microsoft\Office\{officeVersion}\Excel\Options";

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue("RTDThrottleInterval", 200, RegistryValueKind.DWord);
                        key.SetValue("EnableAnimations", 0, RegistryValueKind.DWord);
                        //key.SetValue("DisableHardwareAcceleration", 0, RegistryValueKind.DWord);
                        Console.WriteLine("RTDThrottleInterval set successfully.");
                    }
                    else
                    {
                        using (RegistryKey newKey = Registry.CurrentUser.CreateSubKey(registryPath))
                        {
                            newKey.SetValue("RTDThrottleInterval", 200, RegistryValueKind.DWord);
                            newKey.SetValue("EnableAnimations", 0, RegistryValueKind.DWord);
                            //key.SetValue("DisableHardwareAcceleration", 0, RegistryValueKind.DWord);
                            Console.WriteLine("Key created and value set successfully.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error setting registry value: " + ex.Message);
            }
        }

        private string GetOfficeVersion()
        {
            string officeVersion = "16.0"; // Default to Office 2016/2019/365

            try
            {
                using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"Excel.Application\CurVer"))
                {
                    string curVer = key?.GetValue(null)?.ToString(); // e.g. "Excel.Application.16"
                    if (!string.IsNullOrEmpty(curVer))
                    {
                        officeVersion = curVer.Split('.').Last(); // "16"
                        officeVersion += ".0";
                    }
                }

                return officeVersion;
            }
            catch (Exception ex)
            {
                ApplicationLogger.Log("Failed to detect Excel bitness/version: " + ex.Message);
                return officeVersion; // Fallback assumption
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
