using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Application = Microsoft.Office.Interop.Excel.Application;


namespace thecalcifyRTD
{
    [ComVisible(true)]
    [Guid("A27CE8E8-3BF7-4EB1-A194-9DFE798ABFBB")]
    [ProgId("thecalcify")]
    [ClassInterface(ClassInterfaceType.None)]
    public class thecalcifyRtdServer : IRtdServer
    {
        private IRTDUpdateEvent _callback;
        private Dictionary<int, string[]> _topics;
        private readonly object _lock = new object();

        // 🔹 Store all symbol/field data
        private readonly Dictionary<string, Dictionary<string, object>> _data =
            new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, Dictionary<string, object>> _defaultData =
    new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);


        public int ServerStart(IRTDUpdateEvent callback)
        {
            _callback = callback;
            _topics = new Dictionary<int, string[]>();

            // load init snapshot once
            LoadDefaultDataFromFile();

            try
            {
                var excelApp = GetExcelApp();
                if (excelApp != null)
                    excelApp.EnableAnimations = false;
            }
            catch (Exception)
            {
            }

            // Background pipe listener
            Task.Run(() => StartPipeListener());
            return 1;
        }

        private void StartPipeListener()
        {
            while (true)
            {
                try
                {
                    using (var server = new NamedPipeServerStream("theCalcifyPipe", PipeDirection.In))
                    using (var reader = new StreamReader(server))
                    {
                        server.WaitForConnection();
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var parts = line.Split('|');
                            var symbol = parts[0].Replace(" ▲", "").Replace(" ▼", "").Trim();

                            var fields = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                            foreach (var field in parts.Skip(1))
                            {
                                var kv = field.Split('=');
                                if (kv.Length == 2)
                                    fields[kv[0].Trim()] = kv[1];
                            }

                            lock (_lock)
                            {
                                if (!_data.ContainsKey(symbol))
                                    _data[symbol] = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                                // apply only changed fields (do NOT touch defaults)
                                foreach (var kv in fields)
                                {
                                    _data[symbol][kv.Key] = kv.Value;
                                }
                            }

                            // notify only if this symbol is subscribed
                            if (_topics.Values.Any(t => t[0].Equals(symbol, StringComparison.OrdinalIgnoreCase)))
                            {
                                _callback?.UpdateNotify();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("[Pipe Listener] " + ex.Message);
                }
            }
        }

        public object ConnectData(int topicId, ref Array strings, ref bool newValues)
        {
            if (strings.Length < 2) return "Invalid";

            string symbol = Convert.ToString(strings.GetValue(0))?.Trim();
            string field = Convert.ToString(strings.GetValue(1))?.Trim();

            _topics[topicId] = new[] { symbol, field };

            lock (_lock)
            {
                // Return current value (live if available, else default)
                var val = GetCurrentValue(symbol, field);

                // Optionally hint Excel there may be more values later
                newValues = true;
                return val;
            }
        }

        public void DisconnectData(int topicId) => _topics.Remove(topicId);

        public Array RefreshData(ref int topicCount)
        {
            lock (_lock)
            {
                object[,] data = new object[2, _topics.Count];
                int i = 0;

                foreach (var kvp in _topics)
                {
                    int id = kvp.Key;
                    string symbol = kvp.Value[0];
                    string field = kvp.Value[1];

                    data[0, i] = id;
                    data[1, i] = GetCurrentValue(symbol, field); // <- unified logic
                    i++;
                }

                topicCount = _topics.Count;
                return data;
            }
        }

        public int Heartbeat() => 1;

        public void ServerTerminate()
        {
            _topics?.Clear();
            lock (_lock) _data.Clear();
        }

        private object GetCurrentValue(string symbol, string field)
        {
            // Order: live update first, else default snapshot, else N/A
            if (_data.TryGetValue(symbol, out var live) && live.TryGetValue(field, out var v1))
                return v1 ?? "N/A";

            if (_defaultData.TryGetValue(symbol, out var def) && def.TryGetValue(field, out var v2))
                return v2 ?? "N/A";

            return "N/A";
        }

        private Application GetExcelApp()
        {
            try
            {
                return (Application)
                    Marshal.GetActiveObject("Excel.Application");
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void LoadDefaultDataFromFile()
        {
            string filePath = @"C:\Users\Public\Calcify\initdata.dat";
            if (!File.Exists(filePath)) return;

            try
            {
                var dict = JsonConvert.DeserializeObject<
                    Dictionary<string, Dictionary<string, object>>
                >(File.ReadAllText(filePath));

                if (dict != null)
                {
                    lock (_lock)
                    {
                        _defaultData.Clear();
                        foreach (var kvp in dict)
                            _defaultData[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch
            {
            }
        }
    }
}
