using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Application = Microsoft.Office.Interop.Excel.Application;


namespace thecalcifyRTD
{
    [ComVisible(true)]
    [Guid("A27CE8E8-3BF7-4EB1-A194-9DFE798ABFBB")]
    [ProgId("thecalcify")]
    [ClassInterface(ClassInterfaceType.None)]
    public class CalcifyRtdServer : IRtdServer
    {
        private IRTDUpdateEvent _callback;
        private ConcurrentDictionary<int, (string Symbol, string Field)> _topics;
        private readonly object _callbackLock = new object();
        private static readonly string marketInitDataPath = GetInitDataPath();

        // Use concurrent collections for thread safety
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, object>> _liveData =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, object>> _defaultData =
            new ConcurrentDictionary<string, ConcurrentDictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

        private Thread _pipeListenerThread;
        private bool _isRunning = true;
        private long _lastUpdateTime = 0;
        private const long UPDATE_THROTTLE_MS = 50; // Max 20 updates per second

        public int ServerStart(IRTDUpdateEvent callback)
        {
            _callback = callback;
            _topics = new ConcurrentDictionary<int, (string, string)>();

            LoadDefaultDataFromFile();

            // Start high-performance pipe listener
            _pipeListenerThread = new Thread(PipeListener)
            {
                Priority = ThreadPriority.Highest,
                IsBackground = true
            };
            _pipeListenerThread.Start();

            return 1;
        }

        private void PipeListener()
        {
            while (_isRunning)
            {
                try
                {
                    using (var server = new NamedPipeServerStream(
                        "theCalcifyPipe",
                        PipeDirection.In,
                        -1,
                        PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous | PipeOptions.WriteThrough))
                    {
                        server.WaitForConnection();

                        byte[] buffer = new byte[4096];
                        StringBuilder messageBuilder = new StringBuilder();

                        while (_isRunning && server.IsConnected)
                        {
                            int bytesRead = server.Read(buffer, 0, buffer.Length);
                            if (bytesRead == 0) continue;

                            string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            messageBuilder.Append(chunk);

                            // Process complete messages
                            string fullMessage = messageBuilder.ToString();
                            int messageEnd;
                            while ((messageEnd = fullMessage.IndexOf('\n')) >= 0)
                            {
                                string message = fullMessage.Substring(0, messageEnd).Trim();
                                fullMessage = fullMessage.Substring(messageEnd + 1);

                                if (!string.IsNullOrEmpty(message))
                                {
                                    ProcessMessage(message);
                                }
                            }
                            messageBuilder = new StringBuilder(fullMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Pipe Error] {ex.Message}");
                    Thread.Sleep(100); // Brief pause on error
                }
            }
        }

        private void ProcessMessage(string message)
        {
            try
            {
                var parts = message.Split('|');
                if (parts.Length < 2) return;

                string symbol = parts[0].Replace(" ▲", "").Replace(" ▼", "").Trim();
                var fields = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                foreach (var field in parts.Skip(1))
                {
                    int equalsIndex = field.IndexOf('=');
                    if (equalsIndex > 0)
                    {
                        string key = field.Substring(0, equalsIndex).Trim();
                        string value = field.Substring(equalsIndex + 1).Trim();
                        fields[key] = value;
                    }
                }

                // Update live data
                _liveData.AddOrUpdate(symbol,
                    key => new ConcurrentDictionary<string, object>(fields),
                    (key, existing) =>
                    {
                        foreach (var kv in fields)
                        {
                            existing[kv.Key] = kv.Value;
                        }
                        return existing;
                    });

                // Throttled notification
                long currentTime = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
                if (currentTime - _lastUpdateTime > UPDATE_THROTTLE_MS)
                {
                    _lastUpdateTime = currentTime;
                    NotifyExcelUpdate();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Message Processing Error] {ex.Message}");
            }
        }

        private void NotifyExcelUpdate()
        {
            lock (_callbackLock)
            {
                try
                {
                    _callback?.UpdateNotify();
                }
                catch (COMException ex)
                {
                    Debug.WriteLine($"[Excel Notification Error] {ex.Message}");
                }
            }
        }

        public object ConnectData(int topicId, ref Array strings, ref bool newValues)
        {
            if (strings.Length < 2) return "N/A";

            string symbol = Convert.ToString(strings.GetValue(0))?.Trim();
            string field = Convert.ToString(strings.GetValue(1))?.Trim();

            _topics[topicId] = (symbol, field);

            return GetCurrentValue(symbol, field);
        }

        public Array RefreshData(ref int topicCount)
        {
            var sw = Stopwatch.StartNew();
            var results = new object[2, _topics.Count];
            int index = 0;

            foreach (var topic in _topics)
            {
                results[0, index] = topic.Key;
                results[1, index] = GetCurrentValue(topic.Value.Symbol, topic.Value.Field);
                index++;
            }

            topicCount = _topics.Count;
            return results;
        }

        private object GetCurrentValue(string symbol, string field)
        {
            // Check live data first
            if (_liveData.TryGetValue(symbol, out var liveDict) &&
                liveDict.TryGetValue(field, out var liveValue))
            {
                return liveValue ?? "N/A";
            }

            // Fall back to default data
            if (_defaultData.TryGetValue(symbol, out var defaultDict) &&
                defaultDict.TryGetValue(field, out var defaultValue))
            {
                return defaultValue ?? "N/A";
            }

            return "N/A";
        }

        public void DisconnectData(int topicId)
        {
            _topics.TryRemove(topicId, out _);
        }

        public int Heartbeat()
        {
            return 1;
        }

        public void ServerTerminate()
        {
            _isRunning = false;
            _pipeListenerThread?.Join(1000);

            _topics?.Clear();
            _liveData.Clear();
            _defaultData.Clear();
        }

        private void LoadDefaultDataFromFile()
        {
            if (!File.Exists(marketInitDataPath)) return;

            try
            {
                string cipherText = File.ReadAllText(marketInitDataPath);
                string json = Decrypt(cipherText, "v@d{4NME4sOSywXF");
                var dict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(json);

                if (dict != null)
                {
                    lock (_callbackLock)
                    {
                        _defaultData.Clear();
                        foreach (var kvp in dict)
                            _defaultData[kvp.Key] = new ConcurrentDictionary<string, object>(kvp.Value, StringComparer.OrdinalIgnoreCase);
                    }
                }
            }
            catch
            {
            }
        }

        private static string Decrypt(string cipherText, string passphrase)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(passphrase));
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader(cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        private static string GetInitDataPath()
        {
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            using (var key = baseKey.OpenSubKey(@"SOFTWARE\thecalcify", writable: false))
            {
                string path = key?.GetValue("InitDataPath") as string;
                if (!string.IsNullOrEmpty(path))
                    return path;
            }

            // fallback to DLL folder if registry not set
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "initdata.dat");
        }
    }
}
