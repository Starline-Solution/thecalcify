using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace thecalcify.Helper
{
    public class UpdateAgent
    {
        /// <summary>
        /// Create UpdateAgent Constructor
        /// </summary>
        public UpdateAgent(Form ParentForm)
        {

            try
            {
                string tempBasePath = Path.Combine(APIUrl.TempPath, "thecalcify");

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

        /// <summary>
        /// Uninstall Old Version
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="displayversion"></param>
        public static void UninstallOldVersion(string displayName)
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
        public static void CreateUninstallTask(string uninstallString)
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

                string installerExe = Path.Combine(APIUrl.TempPath, "thecalcify", "thecalcify.exe");
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
