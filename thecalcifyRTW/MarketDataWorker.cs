using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using thecalcify.Shared;
using thecalcifyRTW.Parsers;

namespace thecalcifyRTW
{
    public class MarketDataWorker : BackgroundService
    {
        private readonly ILogger<MarketDataWorker> _logger;
        private Timer _configReloadTimer;

        private HubConnection _signalRConnection;

        // Latest tick per symbol (used to form snapshot, not for queue reset)
        private readonly ConcurrentDictionary<string, byte[]> _latestBySymbol =
            new ConcurrentDictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

        // Producer → Consumer internal channel
        private readonly Channel<string> _incomingChannel = 
            Channel.CreateBounded<string>(
                new BoundedChannelOptions(1000)
                {
                    SingleWriter = false,
                    SingleReader = true,
                    FullMode = BoundedChannelFullMode.DropOldest
                });

        private SharedMemoryQueue _queue;

        private static string[] _currentSymbols = Array.Empty<string>();
        private static readonly string _configPath = RTWAPIUrl.SharedConfigFilePath;

        public static MarketDataWorker Instance { get; private set; }

        public MarketDataWorker(ILogger<MarketDataWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ApplicationLogger.Log("[RTW] Starting MarketDataWorker…");

            // ✅ Create shared memory queue here (NOT in constructor)
            try
            {
                _queue = new SharedMemoryQueue("thecalcifyQueue");
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex, "[RTW] Failed creating SharedMemoryQueue!");
                throw; // Service must stop if IPC fails
            }

            // 🔥 Setup config watcher
            StartConfigWatcher();

            // 🔥 Start publisher loop
            _ = Task.Run(() => PublisherLoop(stoppingToken), stoppingToken);
            _ = Task.Run(() => PipeListenerLoop(stoppingToken), stoppingToken); // ✅ ADD THIS

            // 🔥 Start SignalR logic
            await RunSignalRLoop(stoppingToken);

            //signalRestart(stoppingToken);

        }

        // ----------------------------------------------------------------------
        // SIGNALR
        // ----------------------------------------------------------------------

        private async Task RunSignalRLoop(CancellationToken token)
        {
            string signalrUrl = RTWAPIUrl.ProdUrl;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    ApplicationLogger.Log("[SignalR] Creating connection…");

                    _signalRConnection = new HubConnectionBuilder()
                        .WithUrl(signalrUrl)
                        .WithAutomaticReconnect()
                        .Build();

                    RegisterSignalREvents();

                    ApplicationLogger.Log("[SignalR] Starting connection…");
                    await _signalRConnection.StartAsync(token);

                    ApplicationLogger.Log("[SignalR] Connected. ConnectionId={Id}",
                        _signalRConnection.ConnectionId);

                    await SubscribeAsync(token);

                    // Wait for disconnection
                    await _signalRConnection.WaitForClosedAsync(token);
                }
                catch (Exception ex)
                {
                    ApplicationLogger.LogException(ex, "[SignalR] Error in loop.");
                }

                // Retry delay
                try { await Task.Delay(2000, token); }
                catch { }
            }
        }

        private void RegisterSignalREvents()
        {
            if (_signalRConnection == null)
                return;

            _signalRConnection.Reconnecting += ex =>
            {
                ApplicationLogger.Log($"[SignalR] Reconnecting… {ex} StackTrace is {ex.StackTrace} Message is {ex.Message}");
                return Task.CompletedTask;
            };

            _signalRConnection.Reconnected += id =>
            {
                ApplicationLogger.Log("[SignalR] Reconnected: {Id}", id);
                _ = SubscribeAsync(CancellationToken.None);
                return Task.CompletedTask;
            };

            _signalRConnection.Closed += ex =>
            {
                ApplicationLogger.Log($"[SignalR] Closed… {ex} StackTrace is {ex.StackTrace} Message is {ex.Message}");
                return Task.CompletedTask;
            };

            // Main incoming event
            _signalRConnection.On<string>("excelRate", base64 =>
            {
                try
                {
                    byte[] compressed = Convert.FromBase64String(base64);
                    string json = DecompressGzip(compressed);

                    _incomingChannel.Writer.TryWrite(json);
                }
                catch (Exception ex)
                {
                    ApplicationLogger.LogException(ex, "[SignalR] Failed to process excelRate payload.");
                }
            });
        }

        private string DecompressGzip(byte[] raw)
        {
            using var ms = new MemoryStream(raw);
            using var gz = new GZipStream(ms, CompressionMode.Decompress);
            using var sr = new StreamReader(gz, Encoding.UTF8);
            return sr.ReadToEnd();
        }

        private async Task SubscribeAsync(CancellationToken token)
        {
            if (_signalRConnection == null)
                return;

            if (_currentSymbols.Length == 0)
            {
                ApplicationLogger.Log("[SignalR] No symbols to subscribe.");
                return;
            }

            while (!token.IsCancellationRequested)
            {
                try
                {
                    ApplicationLogger.Log("[SignalR] Sending subscription request…");
                    //Console.WriteLine($"Subscribing to {_currentSymbols.Length} symbols...");
                    
                    await _signalRConnection.InvokeAsync("SymbolLastTick", _currentSymbols, token);
                    await _signalRConnection.InvokeAsync("SubscribeSymbols", _currentSymbols, token);
                    ApplicationLogger.Log($"[SignalR] Subscribed to Count symbols. {_currentSymbols.Length}");
                    return;
                }
                catch (Exception ex)
                {
                    ApplicationLogger.LogException(ex, "[SignalR] Subscribe failed, retrying…");
                    try { await Task.Delay(1000, token); } catch { return; }
                }
            }
        }

        // ----------------------------------------------------------------------
        // PUBLISHER LOOP → WRITE TO SHARED MEMORY
        // ----------------------------------------------------------------------

        private async Task PublisherLoop(CancellationToken token)
        {
            ApplicationLogger.Log("[RTW] Publisher loop started.");

            await foreach (string raw in _incomingChannel.Reader.ReadAllAsync(token))
            {
                if (!MarketDataParser.TryParse(raw, out string symbol, out MarketDataDtoFast dto))
                    continue;

                // Serialize tick → binary
                byte[] bin = TickConverter.ToBinary(dto).ToBytes();

                // Store latest symbol tick (snapshot)
                _latestBySymbol[symbol] = bin;

                // ❗ DO NOT RESET QUEUE — only append
                bool ok = _queue.Write(bin);

                if (!ok)
                {
                    // Queue overflow → optional log
                     ApplicationLogger.Log("[RTW] Queue full, tick dropped.");
                }
            }
        }

        // ----------------------------------------------------------------------
        // CONFIG WATCHER
        // ----------------------------------------------------------------------

        private void StartConfigWatcher()
        {
            var dir = Path.GetDirectoryName(_configPath);
            Directory.CreateDirectory(dir);

            var file = Path.GetFileName(_configPath);

            var watcher = new FileSystemWatcher(dir, file)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime,
                EnableRaisingEvents = true
            };

            watcher.Changed += OnConfigChanged;
            watcher.Created += OnConfigChanged;

            LoadSymbolConfig();
        }

        public void OnConfigChanged(object sender, FileSystemEventArgs e)
        {
            ApplicationLogger.Log("[RTW] Config change detected, scheduling reload…");

            // Debounce: wait 300ms after last change
            _configReloadTimer?.Dispose();
            _configReloadTimer = new Timer(_ =>
            {
                try
                {
                    LoadSymbolConfig();
                    RestartSignalRSubscription();
                }
                catch (Exception ex)
                {
                    ApplicationLogger.LogException(ex);
                }
            }, null, 300, Timeout.Infinite); // ← delay
        }
                
        private async void RestartSignalRSubscription()
        {
            try
            {
                if (_signalRConnection == null)
                    return;

                if (_signalRConnection.State != HubConnectionState.Connected)
                {
                    ApplicationLogger.Log("[RTW] SignalR not connected, skip restart.");
                    return;
                }

                if (_currentSymbols.Length == 0)
                {
                    ApplicationLogger.Log("[RTW] No symbols loaded, skip restart.");
                    return;
                }

                ApplicationLogger.Log("[RTW] Restarting SignalR subscription…");

                //Console.WriteLine($"Restarting SignalR subscription for {_currentSymbols.Length} symbols...");
               
                await _signalRConnection.InvokeAsync("SymbolLastTick", _currentSymbols);
                await _signalRConnection.InvokeAsync("SubscribeSymbols", _currentSymbols);

                ApplicationLogger.Log("[RTW] Subscription restart complete.");
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
            }
        }

        public void LoadSymbolConfig()
        {
            try
            {
                if (!File.Exists(_configPath))
                    return;


                ApplicationLogger.Log($"[RTW] Loading symbol config from {_configPath}…");

                var json = File.ReadAllText(_configPath);
                var list = JsonConvert.DeserializeObject<List<string>>(json);

                if (list == null || list.Count == 0)
                    return;

                _currentSymbols = list.ToArray();
                //ApplicationLogger.Log("[RTW] Loaded {Count} symbols.", _currentSymbols.Length);
                //Console.WriteLine(JsonConvert.SerializeObject(_currentSymbols));
                ApplicationLogger.Log($"[RTW] Loaded Count symbols. {_currentSymbols.Length}");


                _ = ResubscribeAsync();
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex, "[RTW] Error loading symbol config.");
            }
        }

        private async Task ResubscribeAsync()
        {
            try
            {
                if (_signalRConnection == null)
                    return;
                if (_signalRConnection.State != HubConnectionState.Connected)
                    return;

                if (_currentSymbols.Length == 0)
                    return;

                //ApplicationLogger.Log("[RTW] Re-subscribing to {Count} symbols…", _currentSymbols.Length);
                ApplicationLogger.Log($"[RTW] Re-subscribing to Count symbols. {_currentSymbols.Length}");

                //Console.WriteLine($"Resubscribing to {_currentSymbols.Length} symbols...");

                await _signalRConnection.InvokeAsync("SymbolLastTick", _currentSymbols);
                await _signalRConnection.InvokeAsync("SubscribeSymbols", _currentSymbols);

                ApplicationLogger.Log("[RTW] Re-subscribe OK.");
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex, "[RTW] Resubscribe failed.");
            }
        }

        // ----------------------------------------------------------------------
        // STOP
        // ----------------------------------------------------------------------

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            ApplicationLogger.Log("[RTW] Stopping worker…");

            _incomingChannel.Writer.TryComplete();

            if (_signalRConnection != null)
            {
                try { await _signalRConnection.StopAsync(cancellationToken); } catch { }
                try { await _signalRConnection.DisposeAsync(); } catch { }
                _signalRConnection = null;

            }

            await base.StopAsync(cancellationToken);
        }

        private async Task PipeListenerLoop(CancellationToken stoppingToken)
        {
            const string PipeName = "TheCalcifyPipe";

            ApplicationLogger.Log("[PIPE] Listener started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var pipe = new NamedPipeServerStream(
                        PipeName,
                        PipeDirection.In,
                        1,
                        PipeTransmissionMode.Message,
                        PipeOptions.Asynchronous);

                    await pipe.WaitForConnectionAsync(stoppingToken);

                    using var reader = new StreamReader(pipe, Encoding.UTF8);
                    var message = await reader.ReadLineAsync();

                    if (string.IsNullOrWhiteSpace(message))
                        continue;

                    ApplicationLogger.Log($"[PIPE] Received: {message}");

                    if (message == "RESTART")
                    {
                        LoadSymbolConfig();
                    }
                }
                catch (OperationCanceledException)
                {
                    break; // service stopping
                }
                catch (Exception ex)
                {
                    ApplicationLogger.LogException(ex, "[PIPE] Listener error");
                }
            }

            ApplicationLogger.Log("[PIPE] Listener stopped.");
        }

    }

    // ---------------------------------------------------------
    // Extension for waiting
    // ---------------------------------------------------------
    public static class HubConnectionExtensions
    {
        public static Task WaitForClosedAsync(this HubConnection connection, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<object>();

            connection.Closed += error =>
            {
                tcs.TrySetResult(null);
                return Task.CompletedTask;
            };

            token.Register(() => tcs.TrySetCanceled(token));

            return tcs.Task;
        }
    }
}
