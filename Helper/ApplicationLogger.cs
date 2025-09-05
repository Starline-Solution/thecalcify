using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace thecalcify.Helper
{
    public static class ApplicationLogger
    {
        private static readonly string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        private static readonly string logFilePath = Path.Combine(logDirectory, $"log_{DateTime.Now:yyyyMMdd}.txt");

        private static readonly BlockingCollection<string> logQueue = new BlockingCollection<string>();
        private static readonly CancellationTokenSource cts = new CancellationTokenSource();

        static ApplicationLogger()
        {
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);

            // Start background writer
            Task.Factory.StartNew(() =>
            {
                using (var writer = new StreamWriter(logFilePath, true))
                {
                    foreach (var msg in logQueue.GetConsumingEnumerable(cts.Token))
                    {
                        writer.WriteLine(msg);
                        writer.Flush();
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        public static void Log(string message, string level = "INFO")
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss:ff} [{level}] {message}";
            // Add to queue (non-blocking)
            if (!logQueue.IsAddingCompleted)
            {
                logQueue.Add(logEntry);
            }
        }

        public static void LogException(Exception ex)
        {
            string message = $"Exception: {ex.Message}\nStackTrace: {ex.StackTrace}";
            if (ex.InnerException != null)
                message += $"\nInnerException: {ex.InnerException.Message}\nInnerStackTrace: {ex.InnerException.StackTrace}";
            Log(message, "ERROR");
        }

        public static void Shutdown()
        {
            logQueue.CompleteAdding();
            cts.Cancel();
        }
    }
}
