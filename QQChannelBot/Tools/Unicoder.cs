using System.Globalization;
using System.Text.RegularExpressions;

namespace QQChannelBot.Tools
{
    /// <summary>
    /// Unicode编解码器
    /// </summary>
    public static class Unicoder
    {
        private static readonly Regex reUnicode = new(@"\\u([0-9a-fA-F]{4})", RegexOptions.Compiled);
        /// <summary>
        /// Unicode编码(\uxxxx)序列转字符串
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Decode(string s)
        {
            return reUnicode.Replace(s, m =>
            {
                if (short.TryParse(m.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out short c))
                {
                    return "" + (char)c;
                }
                return m.Value;
            });
        }

        private static readonly Regex reUnicodeChar = new(@"[^\u0000-\u00ff]", RegexOptions.Compiled);
        /// <summary>
        /// 字符串转Unicode编码(\uxxxx)序列
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Encode(string s)
        {
            return reUnicodeChar.Replace(s, m => string.Format(@"\u{0:x4}", (short)m.Value[0]));
        }
    }
}
