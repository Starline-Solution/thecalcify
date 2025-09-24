using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace thecalcify.Helper
{
    public class UpdateAgent
    {
        string newInstallerPath = string.Empty;
        string oldInstallerPath = @"C:\Program Files (x86)\thecalcify\thecalcify\thecalcify.exe";

        #region New Setup Version Reader

        [DllImport("msi.dll", SetLastError = true)]
        static extern uint MsiOpenDatabase(string szDatabasePath, IntPtr phPersist, out IntPtr phDatabase);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        static extern int MsiDatabaseOpenViewW(IntPtr hDatabase, [MarshalAs(UnmanagedType.LPWStr)] string szQuery, out IntPtr phView);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        static extern int MsiViewExecute(IntPtr hView, IntPtr hRecord);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        static extern uint MsiViewFetch(IntPtr hView, out IntPtr hRecord);

        [DllImport("msi.dll", CharSet = CharSet.Unicode)]
        static extern int MsiRecordGetString(IntPtr hRecord, int iField, [Out] StringBuilder szValueBuf, ref int pcchValueBuf);

        [DllImport("msi.dll", ExactSpelling = true)]
        static extern uint MsiCloseHandle(IntPtr hAny);

        #endregion New Setup Version Reader

        string displayversion = string.Empty;
        string currentveriosn = string.Empty;

        /// <summary>
        /// Create UpdateAgent Constructor
        /// </summary>
        public UpdateAgent(string token)
        {
            try
            {

                string tempBasePath = Path.Combine(Path.GetTempPath(), "thecalcify");

                // Ensure the folder is fresh
                if (Directory.Exists(tempBasePath))
                {
                    Directory.Delete(tempBasePath, true);
                }
                Directory.CreateDirectory(tempBasePath);

                string tempZipPath = Path.Combine(Path.GetTempPath(), $"update_{Guid.NewGuid()}.zip");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var response = httpClient.GetAsync("http://api.thecalcify.com/setup").Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var zipBytes = response.Content.ReadAsByteArrayAsync().Result;
                        File.WriteAllBytes(tempZipPath, zipBytes);
                    }
                    else
                    {
                        MessageBox.Show($"You Are already to our Upgraded Version", "Version Upgrade", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                // Extract to temp/thecalcify
                ZipFile.ExtractToDirectory(tempZipPath, tempBasePath);

                // Expecting thecalcify.exe inside temp/thecalcify/thecalcify.exe
                string expectedExePath = Path.Combine(tempBasePath, "thecalcify.exe");

                if (!File.Exists(expectedExePath))
                    throw new FileNotFoundException("thecalcify.exe not found at expected path.");

                newInstallerPath = expectedExePath;

                // Read version and decide
                SetupVersionReader(tempBasePath);

                if (IsSoftwareInstalled("thecalcify"))
                {
                    if (Version.Parse(currentveriosn) > Version.Parse(displayversion))
                    {

                        var result = MessageBox.Show("Application Has Newer Version Want to Upgrade", "Update Version", MessageBoxButtons.OKCancel);
                        if (result == DialogResult.OK)
                            UninstallOldVersion("thecalcify", displayversion);
                        else
                            return;
                    }
                    else
                    {
                        DateTime olddateModified = File.GetLastWriteTime(oldInstallerPath);
                        DateTime newdateModified = File.GetLastWriteTime(newInstallerPath);

                        if (olddateModified < newdateModified)
                        {
                            var result = MessageBox.Show("Application Has Newer Version Want to Upgrade", "Update Version", MessageBoxButtons.OKCancel);
                            if (result == DialogResult.OK)
                                UninstallOldVersion("thecalcify", displayversion);
                            else
                                return;
                        }
                        else
                            MessageBox.Show($"You Are already to our Upgraded Version, Which is {currentveriosn}", "Version Upgrade", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    InstallNewVersion(newInstallerPath);
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        /// <summary>
        /// Get Msi Version
        /// </summary>
        /// <param name="msiPath"></param>
        /// <returns>sb</returns>
        /// 
        public string GetMsiVersion(string msiPath)
        {
            IntPtr hDatabase = IntPtr.Zero;
            IntPtr hView = IntPtr.Zero;
            IntPtr hRecord = IntPtr.Zero;

            try
            {
                uint rc = MsiOpenDatabase(msiPath, IntPtr.Zero, out hDatabase);
                if (rc != 0 || hDatabase == IntPtr.Zero) return null;

                string sql = "SELECT `Value` FROM `Property` WHERE `Property`='ProductVersion'";
                int viewRc = MsiDatabaseOpenViewW(hDatabase, sql, out hView);
                if (viewRc != 0 || hView == IntPtr.Zero) return null;

                int execRc = MsiViewExecute(hView, IntPtr.Zero);
                if (execRc != 0) return null;

                uint fetchRc = MsiViewFetch(hView, out hRecord);
                if (fetchRc != 0 || hRecord == IntPtr.Zero) return null;

                // Read value (field 1 because we SELECT Value)
                int bufSize = 256;
                var sb = new StringBuilder(bufSize);
                int pcch = sb.Capacity;
                int recRc = MsiRecordGetString(hRecord, 1, sb, ref pcch);

                const int ERROR_MORE_DATA = 234;
                if (recRc == ERROR_MORE_DATA)
                {
                    sb = new StringBuilder(pcch + 1);
                    pcch = sb.Capacity;
                    recRc = MsiRecordGetString(hRecord, 1, sb, ref pcch);
                }

                if (recRc != 0) return null;
                return sb.ToString();
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                return null; // Return null if exception occurs
            }
            finally
            {
                // Ensure unmanaged resources are released even if error occurs
                if (hRecord != IntPtr.Zero) MsiCloseHandle(hRecord);
                if (hView != IntPtr.Zero) MsiCloseHandle(hView);
                if (hDatabase != IntPtr.Zero) MsiCloseHandle(hDatabase);
            }
        }

        /// <summary>
        /// Setup Version Reader
        /// </summary>
        public void SetupVersionReader(string setupFolder)
        {
            try
            {
                string msiPath = string.Empty;
                string[] files = Directory.GetFiles(setupFolder, "*.msi", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    msiPath = file;
                }
                string version = GetMsiVersion(msiPath);
                ApplicationLogger.Log($"Version {version}");
                if (version != null)
                {
                    currentveriosn = version;
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        /// <summary>
        /// Check exe Installed Or Not For IsSoftwareInstalled Method
        /// </summary>
        /// <param name="displayName"></param>
        /// <returns></returns>
        public bool IsSoftwareInstalled(string displayName)
        {
            try
            {
                // Define both 64-bit and 32-bit uninstall registry paths
                string[] uninstallPaths = new[]
                {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                    @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
                };

                foreach (string path in uninstallPaths)
                {
                    using (RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(path))
                    {
                        if (baseKey == null) continue;

                        foreach (string subKeyName in baseKey.GetSubKeyNames())
                        {
                            using (RegistryKey subKey = baseKey.OpenSubKey(subKeyName))
                            {
                                if (subKey == null) continue;

                                string name = subKey.GetValue("DisplayName") as string;
                                displayversion = subKey.GetValue("DisplayVersion") as string;

                                if (!string.IsNullOrEmpty(name) && name.Contains(displayName))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                return false;
            }
        }

        /// <summary>
        /// Uninstall Old Version
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="displayversion"></param>
        public void UninstallOldVersion(string displayName, string displayversion)
        {
            try
            {
                // Define both 64-bit and 32-bit uninstall registry paths
                string[] uninstallPaths = new[]
                {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                    @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
                };

                foreach (string path in uninstallPaths)
                {
                    using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(path))
                    {
                        foreach (string subKeyName in rk.GetSubKeyNames())
                        {
                            using (RegistryKey subKey = rk.OpenSubKey(subKeyName))
                            {
                                string name = subKey.GetValue("DisplayName") as string;
                                string uninstallString = subKey.GetValue("UninstallString") as string;

                                if (!string.IsNullOrEmpty(name) && name.Contains(displayName))
                                {
                                    if ((string.IsNullOrEmpty(uninstallString)))
                                        ApplicationLogger.Log($"Uninstalling {name}'s Uninstall String is Empty");


                                    uninstallString = uninstallString.Replace("MsiExec.exe /I", "MsiExec.exe /X");

                                    ApplicationLogger.Log($"Uninstalling {name}...");

                                    //Process uninstallProcess = new Process();
                                    //uninstallProcess.StartInfo.FileName = "cmd.exe";
                                    //uninstallProcess.StartInfo.Arguments = "/C " + uninstallString + " /quiet";
                                    //uninstallProcess.StartInfo.UseShellExecute = true;
                                    //uninstallProcess.StartInfo.CreateNoWindow = true;
                                    //uninstallProcess.Start();
                                    //uninstallProcess.WaitForExit();

                                    CreateUninstallTask(uninstallString, Path.GetDirectoryName(newInstallerPath));

                                    ApplicationLogger.Log("Old version uninstalled.");
                                    break;
                                }
                            }
                        }
                    }
                }

                //if (Version.Parse(currentveriosn) > Version.Parse(displayversion))
                //{
                //    InstallNewVersion(newInstallerPath);
                //}
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }

        }

        /// <summary>
        /// Install New Version
        /// </summary>
        /// <param name="installerPath"></param>
        public static void InstallNewVersion(string installerPath)
        {
            try
            {
                ApplicationLogger.Log("Installing new version...");

                Process installProcess = new Process();
                installProcess.StartInfo.FileName = installerPath;
                installProcess.StartInfo.Arguments = "/quiet";  // silent install
                installProcess.StartInfo.UseShellExecute = true;
                installProcess.StartInfo.CreateNoWindow = true;
                installProcess.Start();
                installProcess.WaitForExit();

                ApplicationLogger.Log("New version installed successfully.");

                //MessageBox.Show("Application Update SuccessFully");
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }

        }

        /// <summary>
        /// Create Task Scheduler For Uninstall And Install New Version
        /// </summary>
        /// <param name="uninstallString"></param>
        /// <param name="tempDir"></param>
        public void CreateUninstallTask(string uninstallString, string tempDir)
        {
            string taskAName = "Clacify_UninstallTask";
            string taskBName = "Clacify_InstallTask";
            string uninstallCmd = Path.Combine(tempDir, "uninstall.cmd");
            string installerExe = Path.Combine(tempDir, "thecalcify.exe");

            // Step 1: Create uninstall.cmd file
            File.WriteAllText(uninstallCmd, $@"
                @echo off
                echo Uninstalling existing version...
                {uninstallString} /quiet

                timeout /t 5

                :: Create Task Scheduler B to install new version
                schtasks /Create /TN {taskBName} /TR ""\""{installerExe}\"" /quiet"" /SC ONCE /ST {DateTime.Now.AddMinutes(1):HH:mm} /RL HIGHEST /F

                :: Run install task
                schtasks /Run /TN {taskBName}

                
                timeout /t 5

                :: Delete both tasks
                schtasks /Delete /TN {taskAName} /F
                schtasks /Delete /TN {taskBName} /F

                exit
                ");

            // Step 2: Create Task Scheduler A to run uninstall.cmd
            Process.Start(new ProcessStartInfo
            {
                FileName = "schtasks",
                Arguments = $"/Create /TN {taskAName} /TR \"cmd.exe /c \"\"{uninstallCmd}\"\"\" /SC ONCE /ST {DateTime.Now.AddMinutes(1):HH:mm} /RL HIGHEST /F",
                UseShellExecute = false,
                CreateNoWindow = true,
            })?.WaitForExit();

            // Step 3: Run Task A immediately
            Process.Start(new ProcessStartInfo
            {
                FileName = "schtasks",
                Arguments = $"/Run /TN {taskAName}",
                UseShellExecute = false,
                CreateNoWindow = true
            })?.WaitForExit();
        }

    }
}
