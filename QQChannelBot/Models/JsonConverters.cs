using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

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
}