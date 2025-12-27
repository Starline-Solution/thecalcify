using Newtonsoft.Json;
using System;
using System.Globalization;

namespace thecalcify.Shared
{
    public class MarketDataDtoFast
    {
        public string i;
        public string v;
        public string n;

        [JsonConverter(typeof(NullableDoubleConverter))] public double? o;
        [JsonConverter(typeof(NullableDoubleConverter))] public double? c;
        [JsonConverter(typeof(NullableDoubleConverter))] public double? d;
        [JsonConverter(typeof(NullableDoubleConverter))] public double? atp;

        [JsonConverter(typeof(FlexibleUnixTimeLongConverter))]
        public long t;

        public long bq, tbq, sq, tsq, vt, oi, ltq;
        public double b, a, ltp, h, l;

    }


    public class NullableDoubleConverter : JsonConverter<double?>
    {
        public override double? ReadJson(
            JsonReader reader,
            Type objectType,
            double? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
                return Convert.ToDouble(reader.Value);

            if (reader.TokenType == JsonToken.String)
            {
                var s = reader.Value?.ToString();

                if (string.IsNullOrWhiteSpace(s) || s == "--")
                    return null;

                if (double.TryParse(
                    s,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var result))
                    return result;
            }

            return null;
        }

        public override void WriteJson(
            JsonWriter writer,
            double? value,
            JsonSerializer serializer)
        {
            if (value.HasValue)
                writer.WriteValue(value.Value);
            else
                writer.WriteNull();
        }
    }

    public class FlexibleUnixTimeLongConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(long);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return 0L;

            // numeric unix time
            if (reader.TokenType == JsonToken.Integer)
                return Convert.ToInt64(reader.Value);

            if (reader.TokenType == JsonToken.String)
            {
                var s = reader.Value?.ToString();

                if (string.IsNullOrWhiteSpace(s))
                    return 0L;

                // unix timestamp as string
                if (long.TryParse(s, out var unixMs))
                    return unixMs;

                // formatted datetime
                if (DateTime.TryParse(
                    s,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal,
                    out var dt))
                {
                    return new DateTimeOffset(dt).ToUnixTimeMilliseconds();
                }
            }

            return 0L;
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            writer.WriteValue((long)value);
        }
    }


}
