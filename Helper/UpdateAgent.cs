using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace thecalcify.Helper
{
    public class UpdateAgent
    {
        string newInstallerPath = string.Empty;
        string oldInstallerPath = @"C:\Program Files\thecalcify\thecalcify\thecalcify.exe";

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
        public UpdateAgent(string token, Form ParentForm)
        {
            SplashManager.Show(ParentForm, "Loading", "" +
                "Working on Upgrade...");

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
                        // Hide the splash after UI update
                        SplashManager.Hide();

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
                    try
                    {
                        ApplicationLogger.Log("Software is installed. Checking versions...");
                        ApplicationLogger.Log($"Current Version: {currentveriosn}, New Version: {displayversion}");

                        if (Version.Parse(currentveriosn) > Version.Parse(displayversion))
                        {
                            // Hide the splash after UI update
                            SplashManager.Hide();

                            var result = MessageBox.Show("Application Has Newer Version Want to Upgrade", "Update Version", MessageBoxButtons.OKCancel);
                            if (result == DialogResult.OK)
                            //if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online)
                            //{
                            {
                                SplashManager.Show(ParentForm,"Loading", "We are Upgrading Ourselves");
                                UninstallOldVersion("thecalcify", displayversion);
                                SplashManager.Hide();
                            }
                            //}
                            //else
                            //{

                            //    MessageBox.Show(
                            //        "The process cannot continue while the system is running on battery power.\n\nPlease connect your device to a power source and restart the process.",
                            //        "Power Supply Disconnected",
                            //        MessageBoxButtons.OK,
                            //        MessageBoxIcon.Warning
                            //    );
                            //    return;
                            //}
                            else
                                return;
                        }
                        else
                        {
                            DateTime olddateModified = File.GetLastWriteTime(oldInstallerPath);
                            DateTime newdateModified = File.GetLastWriteTime(newInstallerPath);

                            ApplicationLogger.Log($"Old Version Date Modified: {olddateModified} && New Version Date Modified: {newdateModified}.");

                            if (olddateModified < newdateModified)
                            {
                                // Hide the splash after UI update
                                SplashManager.Hide();

                                var result = MessageBox.Show("Application Has Newer Version Want to Upgrade", "Update Version", MessageBoxButtons.OKCancel);
                                ApplicationLogger.Log("User selected: " + result.ToString());

                                if (result == DialogResult.OK)
                                //if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online)
                                {
                                    SplashManager.Show(ParentForm, "Loading", "We are Upgrading Ourselves");
                                    UninstallOldVersion("thecalcify", displayversion);
                                    SplashManager.Hide();
                                }
                                //else
                                //{
                                //    // Hide the splash after UI update
                                //    SplashManager.Hide();

                                //    MessageBox.Show(
                                //        "The process cannot continue while the system is running on battery power.\n\nPlease connect your device to a power source and restart the process.",
                                //        "Power Supply Disconnected",
                                //        MessageBoxButtons.OK,
                                //        MessageBoxIcon.Warning
                                //    );
                                //    return;
                                //}
                                else
                                    return;
                            }
                            else
                            {
                                // Hide the splash after UI update
                                SplashManager.Hide();

                                MessageBox.Show($"You Are already to our Upgraded Version, Which is {currentveriosn}", "Version Upgrade", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ApplicationLogger.LogException(ex);
                    }
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
            RegistryView[] views = new[] { RegistryView.Registry64, RegistryView.Registry32 };
            string uninstallPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

            foreach (var view in views)
            {
                try
                {
                    using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                    using (var rk = baseKey.OpenSubKey(uninstallPath))
                    {
                        if (rk == null) continue;

                        foreach (string subKeyName in rk.GetSubKeyNames())
                        {
                            using (var subKey = rk.OpenSubKey(subKeyName))
                            {
                                string name = subKey?.GetValue("DisplayName") as string;
                                displayversion = subKey?.GetValue("DisplayVersion") as string;

                                if (!string.IsNullOrEmpty(name) && name.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ApplicationLogger.Log($"Error reading registry ({view}): {ex.Message}");
                }
            }

            ApplicationLogger.Log("Software not found in any registry view.");
            return false;
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
                RegistryView[] views = new[] { RegistryView.Registry64, RegistryView.Registry32 };
                string uninstallPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

                foreach (var view in views)
                {
                    try
                    {
                        using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                        using (var rk = baseKey.OpenSubKey(uninstallPath))
                        {
                            if (rk == null) continue;

                            foreach (string subKeyName in rk.GetSubKeyNames())
                            {
                                using (var subKey = rk.OpenSubKey(subKeyName))
                                {
                                    string name = subKey?.GetValue("DisplayName") as string;
                                    string uninstallString = subKey.GetValue("UninstallString") as string;

                                    if (!string.IsNullOrEmpty(name) && name.Equals(displayName, StringComparison.OrdinalIgnoreCase))
                                    {
                                        ApplicationLogger.Log($"Found installed software: {name}, Version: {displayversion}");

                                        if ((string.IsNullOrEmpty(uninstallString)))
                                            ApplicationLogger.Log($"Uninstalling {name}'s Uninstall String is Empty");

                                        uninstallString = uninstallString.Replace("MsiExec.exe /I", "MsiExec.exe /X");
                                        ApplicationLogger.Log($"Uninstalling {name}...");
                                        CreateUninstallTask(uninstallString, Path.GetDirectoryName(newInstallerPath));
                                        ApplicationLogger.Log("Old version uninstalled.");
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ApplicationLogger.Log($"Error reading registry ({view}): {ex.Message}");
                    }
                }
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
            try
            {
                //string taskAName = "Clacify_UninstallTask";
                //string taskBName = "Clacify_InstallTask";
                //string uninstallCmd = Path.Combine(tempDir, "uninstall.cmd");
                //string installerExe = Path.Combine(tempDir, "thecalcify.exe");
                //string currentExe = @"C:\Program Files\thecalcify\thecalcify\thecalcify.exe";

                //var sb = new StringBuilder();

                //sb.AppendLine("@echo off");
                //sb.AppendLine("echo Uninstalling existing version...");
                //sb.AppendLine($"{uninstallString} /quiet");
                //sb.AppendLine();
                //sb.AppendLine("timeout /t 5");
                //sb.AppendLine();
                //sb.AppendLine(":: Create Task Scheduler B to install new version");
                //sb.AppendLine($"schtasks /Create /TN {taskBName} /TR \"\\\"{installerExe}\\\" /quiet\" /SC ONCE /ST {DateTime.Now.AddMinutes(1):HH:mm:ss} /RL HIGHEST /F /NP");
                //sb.AppendLine();
                //sb.AppendLine(":: Run install task");
                //sb.AppendLine($"schtasks /Run /TN {taskBName}");
                //sb.AppendLine();
                //sb.AppendLine("timeout /t 5");
                //sb.AppendLine();
                //sb.AppendLine(":: Delete both tasks");
                //sb.AppendLine($"schtasks /Delete /TN {taskAName} /F");
                //sb.AppendLine($"schtasks /Delete /TN {taskBName} /F");
                //sb.AppendLine();
                //sb.AppendLine(":: start thecalcify");
                //sb.AppendLine($"start \"\" \"{currentExe}\"");
                //sb.AppendLine();
                //sb.AppendLine("exit");

                //File.WriteAllText(uninstallCmd, sb.ToString(), new UTF8Encoding(false));

                //ApplicationLogger.Log($"Uninstall script created at {sb.ToString()}");

                //// Step 2: Create Task Scheduler A to run uninstall.cmd
                //Process.Start(new ProcessStartInfo
                //{
                //    FileName = "schtasks",
                //    Arguments = $"/Create /TN {taskAName} /TR \"cmd.exe /c \"\"{uninstallCmd}\"\"\" /SC ONCE /ST {DateTime.Now.AddMinutes(1):HH:mm:ss} /RL HIGHEST /F /NP",
                //    UseShellExecute = true,
                //    Verb = "runas", // Triggers UAC
                //})?.WaitForExit();

                //// Step 3: Run Task A immediately
                //Process.Start(new ProcessStartInfo
                //{
                //    FileName = "schtasks",
                //    Arguments = $"/Run /TN {taskAName}",
                //    UseShellExecute = true,
                //    Verb = "runas", // Triggers UAC
                //})?.WaitForExit();

                string installerExe = Path.Combine(tempDir, "thecalcify.exe");
                string currentExe = @"C:\Program Files\thecalcify\thecalcify\thecalcify.exe";

                // Command sequence:
                // 1. Uninstall quietly
                // 2. Wait 5 seconds
                // 3. Run installer quietly
                // 4. Wait 5 seconds
                // 5. Start installed exe
                string commandScript = string.Format(
                    "{0} /quiet && timeout /t 5 && \"{1}\" /quiet && timeout /t 5 && start \"\" \"{2}\"",
                    uninstallString,               // uninstall command (e.g. MsiExec.exe /x {GUID})
                    installerExe.Replace("\"", "\\\""),  // installer path with escaped quotes
                    currentExe.Replace("\"", "\\\"")     // installed app exe to start
                );

                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{commandScript}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                Process.Start(psi)?.WaitForExit();
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }
    }
}
