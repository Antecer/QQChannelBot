namespace System
{
    /// <summary>
    /// 字符串处理工具类
    /// </summary>
    public static class StringTools
    {
        /// <summary>
        /// 替换字符串开始位置的字符串
        /// </summary>
        /// <param name="input">源字符串</param>
        /// <param name="query">查找的串</param>
        /// <param name="newStr">替换的目标串</param>
        /// <param name="ignoreCase">查找字符串时忽略大小写</param>
        /// <returns></returns>
        public static string TrimStartString(this string input, string query, string newStr = "", bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (input.Length < query.Length) return input;
            if (!input.StartsWith(query, ignoreCase, null)) return input;
            return string.Concat(newStr, input.AsSpan(query.Length));
        }
        /// <summary>
        /// 替换字符串末尾位置的字符串
        /// </summary>
        /// <param name="input">源字符串</param>
        /// <param name="query">查找的串</param>
        /// <param name="newStr">替换的目标串</param>
        /// <param name="ignoreCase">查找字符串时忽略大小写</param>
        /// <returns></returns>
        public static string TrimEndString(this string input, string query, string newStr = "", bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (input.Length < query.Length) return input;
            if (!input.EndsWith(query, ignoreCase, null)) return input;
            return string.Concat(input.Remove(input.Length - query.Length), newStr);
        }
        /// <summary>
        /// 替换字符串开始和末尾位置的字符串
        /// </summary>
        /// <param name="input">源字符串</param>
        /// <param name="query">查找的串</param>
        /// <param name="newStr">替换的目标串</param>
        /// <param name="ignoreCase">查找字符串时忽略大小写</param>
        /// <returns></returns>
        public static string TrimString(this string input, string query, string newStr = "", bool ignoreCase = false)
        {
            return input.TrimStartString(query, newStr, ignoreCase).TrimEndString(query, newStr, ignoreCase);
        }
        /// <summary>
        /// 判断字符串是否为空
        /// <para>效果等同于string.IsNullOrWhiteSpace()</para>
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsEmpty(this string? input)
        {
            return string.IsNullOrWhiteSpace(input);
        }
    }
}
