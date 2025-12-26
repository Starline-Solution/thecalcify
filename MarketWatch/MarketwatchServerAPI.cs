using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using thecalcify.Helper;

namespace thecalcify.MarketWatch
{
    public class MarketWatchItem
    {
        public int MarketWatchId { get; set; }
        public string MarketWatchName { get; set; }
        public string ClientId { get; set; }
        public string DeviceId { get; set; }
        public List<string> Symbols { get; set; } = new List<string>();
        public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;
    }

    public class MarketWatchApiResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public List<MarketWatchDto> Data { get; set; }
    }

    public class MarketWatchSaveApiResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public MarketWatchSaveData Data { get; set; }
    }



    public class MarketWatchSaveData
    {
        public int Id { get; set; }
        public string MarketWatchName { get; set; }
        public string ListOfSymbols { get; set; }
        public string LastModified { get; set; }
    }

    public class MarketWatchDto
    {
        public int Id { get; set; }
        public string MarketWatchName { get; set; }
        public string ListOfSymbols { get; set; }   // raw string: "[GOLD,SIlver]"
        public string LastModified { get; set; }
    }

    public class MarketwatchServerAPI : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string BaseUrl = $"{APIUrl.ApplicationURL}api/MarketWatch";  // ✅ Fixed: readonly

        public MarketwatchServerAPI(string authToken)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        }

        public MarketwatchServerAPI(string authToken, HttpClient customHttpClient)
        {
            _httpClient = customHttpClient ?? throw new ArgumentNullException(nameof(customHttpClient));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
        }

        // ✅ FIXED: GET - Handles wrapped response + string array parsing (C# 7.3 compatible)
        public async Task<List<MarketWatchItem>> GetMarketWatchAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/get-marketwatch");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ApplicationLogger.Log($"HTTP Error: {response.StatusCode} - {errorContent}");
                    return new List<MarketWatchItem>();
                }

                var jsonString = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonSerializer.Deserialize<MarketWatchApiResponse>(
                    jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (apiResponse?.Data == null || apiResponse.Data.Count == 0)
                    return new List<MarketWatchItem>();

                var result = new List<MarketWatchItem>();
                foreach (var dto in apiResponse.Data)
                {
                    result.Add(new MarketWatchItem
                    {
                        MarketWatchId = dto.Id,
                        MarketWatchName = dto.MarketWatchName,
                        ClientId = null,  // Not in API response
                        DeviceId = null,  // Not in API response
                        ModifiedDate = ParseLastModified(dto.LastModified),
                        Symbols = ParseSymbols(dto.ListOfSymbols)
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                return new List<MarketWatchItem>();
            }
        }

        public async Task<MarketWatchItem> GetMarketWatchByIdAsync(int marketWatchId)
        {
            var allMarketWatches = await GetMarketWatchAsync();
            return allMarketWatches?.FirstOrDefault(mw => mw.MarketWatchId == marketWatchId);
        }

        public async Task<MarketWatchItem> GetMarketWatchByNameAsync(string marketWatchName)
        {
            var allMarketWatches = await GetMarketWatchAsync();
            return allMarketWatches?.FirstOrDefault(mw =>
                string.Equals(mw.MarketWatchName, marketWatchName, StringComparison.OrdinalIgnoreCase));
        }

        // ✅ FIXED: POST - Matches curl exactly (save-marketwatch)
        public async Task<MarketWatchItem> SaveMarketWatchAsync(MarketWatchItem marketWatch)
        {
            try
            {
                // ✅ Convert List<string> to string format for API (C# 7.3 compatible)
                var apiPayload = new
                {
                    marketWatchName = marketWatch.MarketWatchName,
                    listOfSymbols = marketWatch.Symbols != null && marketWatch.Symbols.Count > 0
                        ? $"{string.Join(",", marketWatch.Symbols)}"
                        : string.Empty
                };

                var jsonString = JsonSerializer.Serialize(apiPayload);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/save-marketwatch", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ApplicationLogger.Log($"POST Error: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                ApplicationLogger.Log($"POST Success: {responseJson}");

                // Parse response and refresh from GET
                var saveResponse = JsonSerializer.Deserialize<MarketWatchSaveApiResponse>(
                    responseJson,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );

                if (saveResponse?.IsSuccess == true && saveResponse.Data?.Id > 0)
                {
                    int savedId = saveResponse.Data.Id;

                    await Task.Delay(300); // optional
                    return await GetMarketWatchByIdAsync(savedId);
                }


                return null;
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                return null;
            }
        }

        // ✅ FIXED: PUT - Matches curl exactly (update-marketwatch/{id})
        public async Task<MarketWatchItem> UpdateMarketWatchAsync(MarketWatchItem marketWatch)
        {
            try
            {
                var apiPayload = new
                {
                    marketWatchName = marketWatch.MarketWatchName,
                    listOfSymbols = marketWatch.Symbols != null && marketWatch.Symbols.Count > 0
                        ? $"[{string.Join(",", marketWatch.Symbols)}]"
                        : "[]"
                };

                var jsonString = JsonSerializer.Serialize(apiPayload);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                ApplicationLogger.Log($"PUT: {BaseUrl}/update-marketwatch/{marketWatch.MarketWatchId}");
                ApplicationLogger.Log($"Payload: {jsonString}");

                var response = await _httpClient.PutAsync($"{BaseUrl}/update-marketwatch/{marketWatch.MarketWatchId}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ApplicationLogger.Log($"PUT Error: {response.StatusCode} - {errorContent}");
                    return null;
                }

                // Refresh from GET after update
                await Task.Delay(500);
                return await GetMarketWatchByIdAsync(marketWatch.MarketWatchId);
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                return null;
            }
        }

        public async Task<bool> DeleteMarketWatchAsync(int marketWatchId)
        {
            try
            {
                ApplicationLogger.Log($"{BaseUrl}/delete-market-watch/{marketWatchId}");
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/delete-marketwatch/{marketWatchId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                ApplicationLogger.LogException(ex);
                return false;
            }
        }

        public async Task<string[]> GetMarketWatchListAsync()
        {
            var allMarketWatches = await GetMarketWatchAsync();
            return allMarketWatches?.Select(mw => mw.MarketWatchName).ToArray() ?? new string[0];
        }

        public void UpdateAuthToken(string newAuthToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", newAuthToken);
        }

        // ✅ FIXED: C# 7.3 Compatible Helpers
        private DateTime ParseLastModified(string lastModified)
        {
            if (!string.IsNullOrEmpty(lastModified) &&
                DateTime.TryParseExact(lastModified, "dd-MM-yyyy HH:mm:ss",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                return dt;
            }
            return DateTime.UtcNow;
        }

        private List<string> ParseSymbols(string listOfSymbols)
        {
            if (string.IsNullOrWhiteSpace(listOfSymbols))
                return new List<string>();

            var trimmed = listOfSymbols.Trim();

            // ✅ C# 7.3: Manual string trimming (no range operators)
            if (trimmed.Length > 0 && trimmed[0] == '[')
                trimmed = trimmed.Substring(1);
            if (trimmed.Length > 0 && trimmed[trimmed.Length - 1] == ']')
                trimmed = trimmed.Substring(0, trimmed.Length - 1);

            var parts = trimmed.Split(',');
            var symbols = new List<string>();

            foreach (var part in parts)
            {
                var symbol = part.Trim();
                if (!string.IsNullOrEmpty(symbol))
                    symbols.Add(symbol);
            }

            return symbols;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
