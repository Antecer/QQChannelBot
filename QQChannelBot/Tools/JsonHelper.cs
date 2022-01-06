namespace System.Text.Json
{
    /// <summary>
    /// Json访问帮助类
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// 查找JSON对象
        /// </summary>
        /// <param name="element"></param>
        /// <param name="name">json的key</param>
        /// <returns></returns>
        public static JsonElement? Get(this JsonElement element, string name)
        {
            if (element.ValueKind != JsonValueKind.Object) return null;
            return element.TryGetProperty(name, out JsonElement value) ? value : null;
        }
        /// <summary>
        /// 索引JSON数组
        /// </summary>
        /// <param name="element"></param>
        /// <param name="index">json的index</param>
        /// <returns></returns>
        public static JsonElement? Get(this JsonElement element, int index)
        {
            if (element.ValueKind != JsonValueKind.Array)
            {
                if (element.ValueKind == JsonValueKind.Object) return element.Get(index.ToString());
                return null;
            }
            JsonElement value = element.EnumerateArray().ElementAtOrDefault(index);
            return value.ValueKind != JsonValueKind.Undefined ? value : null;
        }
    }
}
