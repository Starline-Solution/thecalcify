using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace thecalcify.Helper
{
    public class UpdateAgent
    {
        string newInstallerPath = string.Empty;


        /// <summary>
        /// Create UpdateAgent Constructor
        /// </summary>
        public UpdateAgent(string token, Form ParentForm)
        {

            try
            {
                string tempBasePath = Path.Combine(APIUrl.SystemTempPath, "thecalcify");

                // Ensure the folder is fresh
                bool isUpToDate = true;

                if (Directory.Exists(tempBasePath) && File.Exists(Path.Combine(tempBasePath, "thecalcify.exe")))
                {
                    DateTime installedModified = File.GetLastWriteTime(APIUrl.InstallationPath);
                    DateTime installerModified = File.GetLastWriteTime(Path.Combine(tempBasePath, "thecalcify.exe"));

                    if ((installerModified - installedModified).TotalMinutes > 2)
                    {
                        isUpToDate = false;
                    }
                }

                if (isUpToDate)
                {
                    SplashManager.Hide();
                    MessageBox.Show(
                        "Application is already up to date.",
                        "No Update Available",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return;
                }

                SplashManager.Show(ParentForm, "Working On Upgrade");
                UninstallOldVersion("thecalcify");

                //try
                //{
                //    ApplicationLogger.Log("Software is installed. Checking versions...");
                //    ApplicationLogger.Log($"Current Version: {currentveriosn}, New Version: {displayversion}");

                //    if (Version.Parse(currentveriosn) > Version.Parse(displayversion))
                //    {
                //        // Hide the splash after UI update

                //        var result = MessageBox.Show("Application Has Newer Version Want to Upgrade", "Update Version", MessageBoxButtons.OKCancel);
                //        if (result == DialogResult.OK)
                //        //if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online)
                //        //{
                //        {
                           
                //        }
                //        else
                //            return;
                //    }
                //    else
                //    {
                //        DateTime olddateModified = File.GetLastWriteTime(oldInstallerPath);
                //        DateTime newdateModified = File.GetLastWriteTime(newInstallerPath);

                //        ApplicationLogger.Log($"Old Version Date Modified: {olddateModified} && New Version Date Modified: {newdateModified}.");

                //        if (olddateModified < newdateModified)
                //        {
                //            // Hide the splash after UI update
                //            SplashManager.Hide();

                //            var result = MessageBox.Show("Application Has Newer Version Want to Upgrade", "Update Version", MessageBoxButtons.OKCancel);
                //            ApplicationLogger.Log("User selected: " + result.ToString());

                //            if (result == DialogResult.OK)
                //            //if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online)
                //            {
                //                SplashManager.Show(ParentForm, "Loading", "We are Upgrading Ourselves");
                //                UninstallOldVersion("thecalcify", displayversion);
                //                SplashManager.Hide();
                //            }
                //            else
                //                return;
                //        }
                //        else
                //        {
                //            // Hide the splash after UI update
                //            SplashManager.Hide();

                //            MessageBox.Show($"You Are already to our Upgraded Version, Which is {currentveriosn}", "Version Upgrade", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    ApplicationLogger.LogException(ex);
                //}
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                SplashManager.Hide();
                MessageBox.Show("Proccess Has been failed", "Update Fail", MessageBoxButtons.OK);
            }
        }

        ///// <summary>
        ///// Check For Update
        ///// </summary>
        //public void CheckForUpdate()
        //{
        //    try
        //    {
        //        string localExePath = @"C:\Program Files\thecalcify\thecalcify\thecalcify.exe";

        //        using (var httpClient = new HttpClient())
        //        {
        //            string filepath = APIUrl.LocalVersionPath;
        //            // STEP 1: Read version.json
        //            string json = httpClient.GetStringAsync(APIUrl.SetupVersionPath).Result;


        //            // 3️⃣ Now safely deserialize extracted JSON
        //            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);


        //            string remoteVersion = data.version;
        //            bool forceUpdate = data.forceUpdate;

        //            // STEP 2: Read local app version (based on EXE modified date)
        //            string localVersion = "0";

        //            if (File.Exists(localExePath))
        //            {
        //                DateTime modified = File.GetLastWriteTime(localExePath);
        //                localVersion = modified.ToString("yyyyMMddHHmm");
        //            }

        //            string remoteComparable = remoteVersion.Replace(".", "");
        //            string localComparable = localVersion;

        //            bool updateNeeded =
        //                forceUpdate ||
        //                string.Compare(remoteComparable, localComparable) > 0;

        //            // STEP 3: If no update needed → do nothing
        //            if (!updateNeeded)
        //                return;

        //            SplashManager.Hide();

        //            // STEP 4: Ask user for permission
        //            DialogResult userChoice = MessageBox.Show(
        //                "A new update is available. Do you want to update now?",
        //                "Update Available",
        //                MessageBoxButtons.YesNo,
        //                MessageBoxIcon.Question);

        //            if (userChoice == DialogResult.Yes)
        //            {
        //                // STEP 5: Call your existing download + extract method
        //                DownloadAndExtract();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Update check failed: " + ex.Message);
        //    }
        //}

        ///// <summary>
        ///// Downloads and extracts the setup files.
        ///// </summary>
        //public void DownloadAndExtract()
        //{
        //    try
        //    {
        //        string baseFolder = ;

        //        // Ensure base folder exists
        //        if (!Directory.Exists(baseFolder))
        //            Directory.CreateDirectory(baseFolder);

        //        string tempZipPath = Path.Combine(baseFolder, "thecalcify.zip");
        //        string extractPath = Path.Combine(baseFolder, "thecalcify");

        //        using (var httpClient = new HttpClient())
        //        {
        //            Stopwatch sw = new Stopwatch();
        //            sw.Start();

        //            var response = httpClient.GetAsync(APIUrl.SetupUrlPath).Result;

        //            sw.Stop();
                    
        //            ApplicationLogger.Log($"Download completed in {sw.ElapsedMilliseconds} ms");

        //            if (response.IsSuccessStatusCode)
        //            {
        //                var zipBytes = response.Content.ReadAsByteArrayAsync().Result;

        //                // If ZIP exists, delete it (avoids lock + access denied)
        //                if (File.Exists(tempZipPath))
        //                {
        //                    File.SetAttributes(tempZipPath, FileAttributes.Normal);
        //                    File.Delete(tempZipPath);
        //                }

        //                // Save ZIP file
        //                File.WriteAllBytes(tempZipPath, zipBytes);

        //                // Rebuild extract folder
        //                if (Directory.Exists(extractPath))
        //                    Directory.Delete(extractPath, true);

        //                Directory.CreateDirectory(extractPath);

        //                // Extract ZIP
        //                ZipFile.ExtractToDirectory(tempZipPath, extractPath);

        //                newInstallerPath = extractPath;

        //                UninstallOldVersion("thecalcify");

        //            }
        //            else
        //            {
        //                MessageBox.Show("Already updated.");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error: " + ex.Message);
        //    }
        //}

        ///// <summary>
        ///// Get Msi Version
        ///// </summary>
        ///// <param name="msiPath"></param>
        ///// <returns>sb</returns>
        ///// 
        //public string GetMsiVersion(string msiPath)
        //{
        //    IntPtr hDatabase = IntPtr.Zero;
        //    IntPtr hView = IntPtr.Zero;
        //    IntPtr hRecord = IntPtr.Zero;

        //    try
        //    {
        //        uint rc = MsiOpenDatabase(msiPath, IntPtr.Zero, out hDatabase);
        //        if (rc != 0 || hDatabase == IntPtr.Zero) return null;

        //        string sql = "SELECT `Value` FROM `Property` WHERE `Property`='ProductVersion'";
        //        int viewRc = MsiDatabaseOpenViewW(hDatabase, sql, out hView);
        //        if (viewRc != 0 || hView == IntPtr.Zero) return null;

        //        int execRc = MsiViewExecute(hView, IntPtr.Zero);
        //        if (execRc != 0) return null;

        //        uint fetchRc = MsiViewFetch(hView, out hRecord);
        //        if (fetchRc != 0 || hRecord == IntPtr.Zero) return null;

        //        // Read value (field 1 because we SELECT Value)
        //        int bufSize = 256;
        //        var sb = new StringBuilder(bufSize);
        //        int pcch = sb.Capacity;
        //        int recRc = MsiRecordGetString(hRecord, 1, sb, ref pcch);

        //        const int ERROR_MORE_DATA = 234;
        //        if (recRc == ERROR_MORE_DATA)
        //        {
        //            sb = new StringBuilder(pcch + 1);
        //            pcch = sb.Capacity;
        //            recRc = MsiRecordGetString(hRecord, 1, sb, ref pcch);
        //        }

        //        if (recRc != 0) return null;
        //        return sb.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        ApplicationLogger.LogException(ex);
        //        return null; // Return null if exception occurs
        //    }
        //    finally
        //    {
        //        // Ensure unmanaged resources are released even if error occurs
        //        if (hRecord != IntPtr.Zero) MsiCloseHandle(hRecord);
        //        if (hView != IntPtr.Zero) MsiCloseHandle(hView);
        //        if (hDatabase != IntPtr.Zero) MsiCloseHandle(hDatabase);
        //    }
        //}

        ///// <summary>
        ///// Setup Version Reader
        ///// </summary>
        //public void SetupVersionReader(string setupFolder)
        //{
        //    try
        //    {
        //        string msiPath = string.Empty;
        //        string[] files = Directory.GetFiles(setupFolder, "*.msi", SearchOption.AllDirectories);
        //        foreach (string file in files)
        //        {
        //            msiPath = file;
        //        }

        //        string version = GetMsiVersion(msiPath);
        //        ApplicationLogger.Log($"Version {version}");
        //        if (version != null)
        //        {
        //            currentveriosn = version;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ApplicationLogger.LogException(ex);
        //    }
        //}

        ///// <summary>
        ///// Check exe Installed Or Not For IsSoftwareInstalled Method
        ///// </summary>
        ///// <param name="displayName"></param>
        ///// <returns></returns>
        //public bool IsSoftwareInstalled(string displayName)
        //{
        //    RegistryView[] views = new[] { RegistryView.Registry64, RegistryView.Registry32 };
        //    string uninstallPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

        //    foreach (var view in views)
        //    {
        //        try
        //        {
        //            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
        //            using (var rk = baseKey.OpenSubKey(uninstallPath))
        //            {
        //                if (rk == null) continue;

        //                foreach (string subKeyName in rk.GetSubKeyNames())
        //                {
        //                    using (var subKey = rk.OpenSubKey(subKeyName))
        //                    {
        //                        string name = subKey?.GetValue("DisplayName") as string;
        //                        displayversion = subKey?.GetValue("DisplayVersion") as string;

        //                        if (!string.IsNullOrEmpty(name) && name.Equals(displayName, StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            return true;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            ApplicationLogger.Log($"Error reading registry ({view}): {ex.Message}");
        //        }
        //    }

        //    ApplicationLogger.Log("Software not found in any registry view.");
        //    return false;
        //}

        /// <summary>
        /// Uninstall Old Version
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="displayversion"></param>
        public void UninstallOldVersion(string displayName)
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
                                        ApplicationLogger.Log($"Found installed software: {name} ");

                                        if ((string.IsNullOrEmpty(uninstallString)))
                                            ApplicationLogger.Log($"Uninstalling {name}'s Uninstall String is Empty");

                                        uninstallString = uninstallString.Replace("MsiExec.exe /I", "MsiExec.exe /X");
                                        ApplicationLogger.Log($"Uninstalling {name}...");
                                        CreateUninstallTask(uninstallString);
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
        public void CreateUninstallTask(string uninstallString)
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

                string installerExe = Path.Combine(APIUrl.SystemTempPath, "thecalcify", "thecalcify.exe");
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
