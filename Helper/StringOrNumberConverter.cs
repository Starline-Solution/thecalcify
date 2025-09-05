using System;
using System.Text.Json;
using System.Text.Json.Serialization;

public class StringOrNumberConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return reader.GetString();
        else if (reader.TokenType == JsonTokenType.Number)
            return reader.GetDecimal().ToString(); // or GetDouble()
        else if (reader.TokenType == JsonTokenType.Null)
            return "N/A";
        else
            throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
