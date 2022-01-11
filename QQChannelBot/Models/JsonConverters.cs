using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using QQChannelBot.Bot.SocketEvent;

namespace QQChannelBot.Models
{
    /// <summary>
    /// JSON序列化时将 bool 转换为 int<br/>
    /// JSON反序列化时将 int 转换为 bool
    /// </summary>
    public class BoolToInt32Converter : JsonConverter<bool>
    {
        /// <summary>
        /// 序列化JSON时 bool 转 int
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value ? 1 : 0);
        }
        /// <summary>
        /// 反序列化JSON时 int 转 bool
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.Number => reader.TryGetInt32(out int num) && Convert.ToBoolean(num),
                _ => false,
            };
        }
    }

    /// <summary>
    /// JSON序列化时将 Color 转换为 UInt32<br/>
    /// JSON反序列化时将 UInt32 转换为 Color
    /// </summary>
    public class ColorToUint32Converter : JsonConverter<Color>
    {
        /// <summary>
        /// 序列化JSON时 Color 转 int
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue((uint)value.ToArgb());
        }
        /// <summary>
        /// 反序列化JSON时 int 转 Color
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.Number => Color.FromArgb((int)reader.GetUInt32()),
                _ => Color.Black,
            };
        }
    }

    /// <summary>
    /// JSON序列化时将 RemindType 转换为 StringNumber<br/>
    /// JSON反序列化时将 StringNumber 转换为 RemindType
    /// </summary>
    public class RemindTypeToStringNumberConverter : JsonConverter<RemindType>
    {
        /// <summary>
        /// JSON序列化时将 RemindType 转 StringNumber
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, RemindType value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("D"));
        }
        /// <summary>
        /// JSON反序列化时将 StringNumber 转 RemindType
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override RemindType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            int numCode = reader.TokenType switch
            {
                JsonTokenType.String => int.TryParse(reader.GetString(), out int value) ? value : 0,
                JsonTokenType.Number => reader.GetInt32(),
                _ => 0
            };
            return Enum.GetNames(typeof(RemindType)).Length > numCode ? (RemindType)numCode : RemindType.Never;
        }
    }

    /// <summary>
    /// JSON序列化JSON时将 DateTime 转换为 Timestamp<br/>
    /// JSON反序列化JSON时将 Timestamp 转换为 DateTime
    /// </summary>
    public class DateTimeToStringTimestamp : JsonConverter<DateTime>
    {
        /// <summary>
        /// 序列化JSON时 DateTime 转 Timestamp
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue($"{value:yyyy-MM-ddTHH:mm:sszzz}");
        }
        /// <summary>
        /// 反序列化JSON时 Timestamp 转 DateTime
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    return DateTime.MinValue.AddMilliseconds(reader.GetDouble());
                case JsonTokenType.String:
                    string timeStamp = reader.GetString() ?? "0";
                    Match timeMatch = Regex.Match(timeStamp, @"^\d+$");
                    if (timeMatch.Success)
                    {
                        double offsetVal = double.Parse(timeMatch.Groups[0].Value);
                        return timeStamp.Length <= 10 ? DateTime.MinValue.AddSeconds(offsetVal) : DateTime.MinValue.AddMilliseconds(offsetVal);
                    }
                    else
                    {
                        return DateTime.TryParse(timeStamp, out DateTime result) ? result : DateTime.MinValue;
                    }
                default:
                    return DateTime.MinValue;
            }
        }
    }

    /// <summary>
    /// JSON序列化时将 Intent 转换为 StringArray<br/>
    /// JSON反序列化时将 StringArray 转换为 Intent
    /// </summary>
    public class IntentToStringArrayConverter : JsonConverter<Intent>
    {
        /// <summary>
        /// JSON序列化时将 RemindType 转 StringNumber
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public override void Write(Utf8JsonWriter writer, Intent value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            value.ToString().Split(',').ToList().ForEach(f => writer.WriteStringValue(f.Trim()));
            writer.WriteEndArray();
        }
        /// <summary>
        /// JSON反序列化时将 StringNumber 转 RemindType
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="typeToConvert"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public override Intent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Intent intents = DefaultIntents.Public;
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    int num = reader.GetInt32();
                    if (0 < num) intents = (Intent)num;
                    break;
                case JsonTokenType.String:
                    string? str = reader.GetString();
                    if (!string.IsNullOrWhiteSpace(str)) intents = Enum.Parse<Intent>(str.ToUpper());
                    break;
                case JsonTokenType.StartArray:
                    intents = 0;
                    while (reader.Read())
                    {
                        switch (reader.TokenType)
                        {
                            case JsonTokenType.String:
                                string? s = reader.GetString();
                                if (!string.IsNullOrWhiteSpace(s)) intents |= Enum.Parse<Intent>(s.ToUpper());
                                break;
                            case JsonTokenType.Number:
                                intents |= (Intent)reader.GetInt32();
                                break;
                            default:
                                reader.Skip();
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
            return intents;
        }
    }
}