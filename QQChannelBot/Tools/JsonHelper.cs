namespace System.Text.Json
{
    public static class JsonHelper
    {
        /// <summary>
        /// 查找JSON对象
        /// </summary>
        /// <param name="name">json的key</param>
        /// <returns></returns>
        //public static JsonElement? Get(this JsonElement? element, string name) => element?.Get(name);
        /// <summary>
        /// 查找JSON对象
        /// </summary>
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
        /// <param name="index">json的index</param>
        /// <returns></returns>
        //public static JsonElement? Get(this JsonElement? element, int index) => element?.Get(index);
        /// <summary>
        /// 索引JSON数组
        /// </summary>
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
