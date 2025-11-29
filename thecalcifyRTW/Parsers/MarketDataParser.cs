using Newtonsoft.Json;
using thecalcify.Shared;

namespace thecalcifyRTW.Parsers
{
    public static class MarketDataParser
    {
        public static bool TryParse(string json, out string symbol, out MarketDataDtoFast dto)
        {
            symbol = null;
            dto = null;

            try
            {
                dto = JsonConvert.DeserializeObject<MarketDataDtoFast>(json);
                if (dto == null || string.IsNullOrEmpty(dto.i))
                    return false;

                symbol = dto.i;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
