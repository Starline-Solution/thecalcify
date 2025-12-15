using System;
using System.IO;

namespace thecalcify_Update_Service
{
    public static class ApplicationLogger
    {
        private static readonly string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static readonly object _lock = new object();

        static ApplicationLogger()
        {
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);
        }

        public static void Log(string message, string level = "INFO")
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss:ff} [{level} Update Service] {message}";
            string logFilePath = Path.Combine(logDirectory, $"log_{DateTime.Now:yyyyMMdd}.txt");

            lock (_lock)
            {
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
        }

        public static void LogException(Exception ex, string additionalInfo = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrEmpty(additionalInfo))
                message = $"\nAdditionalInfo: {additionalInfo}";

            message += $"Exception: {ex.Message}\nStackTrace: {ex.StackTrace}";

            if (ex.InnerException != null)
                message += $"\nInnerException: {ex.InnerException.Message}\nInnerStackTrace: {ex.InnerException.StackTrace}";

            
            Log(message, "ERROR");
        }
    }
}
