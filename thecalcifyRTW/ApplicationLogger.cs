using System;
using System.IO;

namespace thecalcifyRTW
{
    public static class ApplicationLogger
    {
        private static readonly string logDirectory =
            @"C:\Program Files\thecalcify\thecalcify\Logs\thecalcifyRTW";

        private static readonly object _lock = new object();

        static ApplicationLogger()
        {
            try
            {
                // Will ONLY succeed if folder already exists and permissions are correct
                Directory.CreateDirectory(logDirectory);
            }
            catch { }
        }

        public static void Log(string message, string level = "INFO")
        {
            try
            {
                string logFilePath =
                    Path.Combine(logDirectory, $"log_{DateTime.Now:yyyyMMdd}.txt");

                string logEntry =
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss:ff} [{level}] {message}";

                lock (_lock)
                {
                    File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
                }
            }
            catch
            {
                // Never crash service
            }
        }

        public static void LogException(Exception ex, string context = null)
        {
            string msg =
                $"Exception: {ex.Message}\nStackTrace: {ex.StackTrace}";

            if (ex.InnerException != null)
            {
                msg +=
                    context != null ? $"\nContext: {context}" : string.Empty +
                    $"\nInnerException: {ex.InnerException.Message}" +
                    $"\nInnerStackTrace: {ex.InnerException.StackTrace}";
            }

            Log(msg, "ERROR");
        }
    }
}
