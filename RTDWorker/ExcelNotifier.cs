using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;

namespace thecalcify.Helper
{
    public static class ExcelNotifier
    {
        private static NamedPipeClientStream _client;
        private static StreamWriter _writer;

        private static readonly object _lock = new object();
        private static bool _connected = false;

        public static bool IsConnected => _connected && _client != null && _client.IsConnected;

        private static DateTime _lastConnectAttempt = DateTime.MinValue;
        private static Timer excelTimer;

        // =======================================================
        // START WINFORMS TIMER (CALL THIS ONCE FROM Home.cs)
        // =======================================================
        public static void StartExcelMonitor()
        {
            excelTimer = new Timer();
            excelTimer.Interval = 1000; // 1 sec
            excelTimer.Tick += ExcelTimer_Tick;
            excelTimer.Start();
        }

        // =======================================================
        // TIMER LOGIC: CHECK IF ANY WORKBOOK USING RTD IS OPEN
        // =======================================================
        private static void ExcelTimer_Tick(object sender, EventArgs e)
        {
            bool isExcelOpen = IsExcelFileOpen();

            if (isExcelOpen)
            {
                if (!IsConnected)
                {
                    if (TryReconnect() && IsConnected)
                    {
                        // Push full snapshot ONLY once after reconnect
                        PushFullSnapshotToExcel();
                    }
                }
            }
            else
            {
                Disconnect();
            }
        }

        // =======================================================
        // DETECT ANY WORKBOOK USING =RTD("thecalcify"...)
        // (Automatically works for ANY filename)
        // =======================================================
        private static bool IsExcelFileOpen()
        {
            try
            {
                var excel = (Microsoft.Office.Interop.Excel.Application)
                    System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");

                foreach (Workbook wb in excel.Workbooks)
                {
                    if (wb.Name.Equals("thecalcify.xlsx", StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            catch
            {
                // Excel not running
            }

            return false;
        }

        // =======================================================
        // SEND EACH TICK (CALLED FROM LastTickStore.ExcelPublish)
        // =======================================================
        public static void NotifyExcel(string symbol, Dictionary<string, object> dict)
        {
            try
            {
                lock (_lock)
                {
                    if (!IsConnected)
                    {
                        if ((DateTime.Now - _lastConnectAttempt).TotalSeconds >= 2)
                        {
                            _lastConnectAttempt = DateTime.Now;
                            TryReconnect();
                        }
                    }

                    if (IsConnected && _writer != null)
                    {
                        string fields = string.Join("|", dict.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                        string msg = $"{symbol}|{fields}";

                        _writer.WriteLine(msg);
                    }
                }
            }
            catch
            {
                Disconnect();
            }
        }

        // =======================================================
        // PIPE CONNECT (RUNS WHEN EXCEL OPENS)
        // =======================================================
        private static bool TryReconnect()
        {
            try
            {
                _client?.Dispose();

                _client = new NamedPipeClientStream(
                    ".",
                    "theCalcifyPipe",
                    PipeDirection.Out,
                    PipeOptions.Asynchronous);

                _client.Connect(300);

                _writer = new StreamWriter(_client) { AutoFlush = true };

                _connected = true;
                ApplicationLogger.Log("[ExcelNotifier] Connected to RTD pipe.");

                return true;
            }
            catch
            {
                _connected = false;
                _client = null;
                _writer = null;
                return false;
            }
        }

        // =======================================================
        // SEND ENTIRE SNAPSHOT ONCE AFTER RECONNECT
        // =======================================================
        public static void PushFullSnapshotToExcel()
        {
            if (!IsConnected || _writer == null) return;

            var all = LastTickStore.GetAll();

            foreach (var kv in all)
            {
                string symbol = kv.Key;
                var dict = kv.Value;

                string fields = string.Join("|", dict.Select(k => $"{k.Key}={k.Value}"));
                string msg = $"{symbol}|{fields}";

                try
                {
                    _writer.WriteLine(msg);
                }
                catch
                {
                    Disconnect();
                    break;
                }
            }
        }

        // =======================================================
        // DISCONNECT PIPE
        // =======================================================
        public static void Disconnect()
        {
            try
            {
                _connected = false;
                _client?.Dispose();
                _client = null;
                _writer = null;
            }
            catch { }
        }
    }

    // =======================================================
    // LAST TICK SNAPSHOT STORE (THREAD SAFE)
    // =======================================================
    public static class LastTickStore
    {
        private static readonly object _lock = new object();
        private static readonly Dictionary<string, Dictionary<string, object>> _last =
            new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

        public static Dictionary<string, object> UpdateAndGet(string symbol, Dictionary<string, object> newFields)
        {
            lock (_lock)
            {
                if (!_last.TryGetValue(symbol, out var snapshot))
                {
                    snapshot = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    _last[symbol] = snapshot;
                }

                foreach (var kv in newFields)
                    snapshot[kv.Key] = kv.Value;

                // RETURN DEEP COPY
                return new Dictionary<string, object>(snapshot);
            }
        }

        public static Dictionary<string, Dictionary<string, object>> GetAll()
        {
            lock (_lock)
            {
                return _last.ToDictionary(
                    kv => kv.Key,
                    kv => new Dictionary<string, object>(kv.Value),
                    StringComparer.OrdinalIgnoreCase
                );
            }
        }

        // Called per tick
        public static void ExcelPublish(MarketDataDto dto)
        {
            try
            {
                var update = new Dictionary<string, object>
                {
                    { "Bid", dto.b },
                    { "Ask", dto.a },
                    { "LTP", dto.ltp },
                    { "High", dto.h },
                    { "Low", dto.l },
                    { "Open", dto.o },
                    { "Close", dto.c },
                    { "Volume", dto.vt },
                    { "BidSize", dto.bq },
                    { "AskSize", dto.sq },
                    { "TBQ", dto.tbq },
                    { "TSQ", dto.tsq },
                    { "Time", Common.TimeStampConvert(dto.t) }
                };

                var fullSnapshot = UpdateAndGet(dto.i, update);

                ExcelNotifier.NotifyExcel(dto.i, fullSnapshot);
            }
            catch { }
        }
    }
}
