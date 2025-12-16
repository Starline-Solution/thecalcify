using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Net.Http;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Timers;

namespace thecalcify_Update_Service
{
    public partial class thecalcifyUpdate : ServiceBase
    {
        readonly Timer updateTimer = new Timer();

        public thecalcifyUpdate()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ApplicationLogger.Log("thecalcify Update Service started.");

            ApplicationLogger.Log($"Extraction completed at {UpdateApiUrl.TempPath}");

            updateTimer.Interval = 1000 * 60 * 3; // 20 minutes
            updateTimer.Elapsed += CheckForUpdateElapsed;
            updateTimer.Start();

            // Run initial update check in background
            Task.Run(() => CheckForUpdate());
        }

        protected override void OnStop()
        {
            ApplicationLogger.Log("thecalcify Update Service stopped.");

            updateTimer.Stop();
        }

        public void StartDebug(string[] args)
        {
            OnStart(args);
        }

        public void StopDebug()
        {
            OnStop();
        }

        private static void CheckForUpdateElapsed(object sender, ElapsedEventArgs e)
        {
            Task.Run(() => CheckForUpdate());
        }

        /// <summary>
        /// Check For Update
        /// </summary>
        public static void CheckForUpdate()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // STEP 1: Read version.json
                    string json = httpClient.GetStringAsync(UpdateApiUrl.SetupVersionPath).Result;


                    // 3️⃣ Now safely deserialize extracted JSON
                    dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);


                    string remoteVersion = data.version;
                    bool forceUpdate = data.forceUpdate;

                    // STEP 2: Read local app version (based on EXE modified date)
                    string localVersion = "0";

                    if (File.Exists(UpdateApiUrl.InstallationPath))
                    {
                        DateTime modified = File.GetLastWriteTime(UpdateApiUrl.InstallationPath);
                        localVersion = modified.ToString("yyyyMMddHHmm");
                    }

                    string remoteComparable = remoteVersion.Replace(".", "");
                    string localComparable = localVersion;

                    bool updateNeeded =
                        forceUpdate ||
                        string.Compare(remoteComparable, localComparable) > 0;

                    // STEP 3: If no update needed → do nothing
                    if (!updateNeeded)
                        return;


                    if (File.Exists(Path.Combine(UpdateApiUrl.TempPath, "thecalcify", "thecalcify.exe")))
                    {
                        DateTime installedModified = File.GetLastWriteTime(Path.Combine(UpdateApiUrl.TempPath, "thecalcify", "thecalcify.exe"));

                        // Parse remote version string (yyyyMMddHHmm)
                        if (DateTime.TryParseExact(
                                remoteComparable,
                                "yyyyMMddHHmm",
                                null,
                                System.Globalization.DateTimeStyles.None,
                                out DateTime remoteVersionTime))
                        {
                            TimeSpan diff = remoteVersionTime - installedModified;
                            double minutesDiff = Math.Abs(diff.TotalMinutes);

                            // Treat small timestamp differences as same version
                            bool effectivelySameVersion = minutesDiff <= 2;

                            // CASE 1: Same version / remote version is older then downloaded → skip
                            if (effectivelySameVersion || remoteVersionTime < installedModified)
                            {
                                ApplicationLogger.Log(
                                    "Installer version matches local version. Skipping download.");
                                return;
                            }

                            // CASE 2: Remote is newer → continue download
                            ApplicationLogger.Log(
                                "Remote installer is newer. Proceeding with download.");
                        }
                    }



                    // Step 5: Download and extract setup files
                    DownloadAndExtract(data.fileUrl.ToString(), forceUpdate);
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex, "Error during CheckForUpdate");
            }
        }

        /// <summary>
        /// Downloads and extracts the setup files.
        /// </summary>
        public static void DownloadAndExtract(string fileUrl, bool forceUpdate)
        {
            try
            {
                string baseFolder = UpdateApiUrl.TempPath;

                // Ensure base folder exists
                if (!Directory.Exists(baseFolder))
                    Directory.CreateDirectory(baseFolder);

                string tempZipPath = Path.Combine(baseFolder, "thecalcify.zip");
                string extractPath = Path.Combine(baseFolder, "thecalcify");

                using (var httpClient = new HttpClient())
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    httpClient.Timeout = TimeSpan.FromMinutes(10); // Set timeout to 10 minutes
                    var response = httpClient.GetAsync(fileUrl).Result;

                    sw.Stop();

                    ApplicationLogger.Log($"Download completed in {sw.ElapsedMilliseconds} ms");

                    if (response.IsSuccessStatusCode)
                    {
                        var zipBytes = response.Content.ReadAsByteArrayAsync().Result;

                        // If ZIP exists, delete it (avoids lock + access denied)
                        if (File.Exists(tempZipPath))
                        {
                            File.SetAttributes(tempZipPath, FileAttributes.Normal);
                            File.Delete(tempZipPath);
                        }

                        // Save ZIP file
                        File.WriteAllBytes(tempZipPath, zipBytes);

                        // Rebuild extract folder
                        if (Directory.Exists(extractPath))
                            Directory.Delete(extractPath, true);

                        Directory.CreateDirectory(extractPath);

                        // Extract ZIP
                        ZipFile.ExtractToDirectory(tempZipPath, extractPath);

                        ApplicationLogger.Log($"Extraction completed at {extractPath}");
                        ApplicationLogger.Log($"Downloaded and extracted setup files from {fileUrl}");
                        ApplicationLogger.Log($"tempZip file located at: {tempZipPath}");

                        // Rename setup.exe to thecalcify.exe
                        if (File.Exists(Path.Combine(extractPath, "setup.exe")))
                        {
                            File.Move(Path.Combine(extractPath, "setup.exe"), Path.Combine(extractPath, "thecalcify.exe"));
                        }

                        // STEP 6: Send Signal to thecalcify Application if forceUpdate is true
                        if (forceUpdate)
                        {
                            try
                            {
                                SendSignal("Update Downloaded");
                            }
                            catch (Exception)
                            {
                                ApplicationLogger.Log($"Application Not Open and File Download at {DateTime.Now}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex, "Error during DownloadAndExtract");
            }
        }

        /// <summary>
        /// Send Signal to thecalcify Application
        /// </summary>
        /// <param name="message"></param>
        public static void SendSignal(string message)
        {
            using (var client = new NamedPipeClientStream(
                ".",
                "thecalcifyUpdatePipe",
                PipeDirection.InOut))
            {
                client.Connect(3000);

                using (var writer = new StreamWriter(client) { AutoFlush = true })
                using (var reader = new StreamReader(client))
                {
                    writer.WriteLine(message);

                    // Optional response
                    string response = reader.ReadLine();
                }
            }
        }
    }
}
