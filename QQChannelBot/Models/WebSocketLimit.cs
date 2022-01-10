using System.Text.Json.Serialization;

namespace QQChannelBot.Models
{
    /// <summary>
    /// WebSocket连接分片信息
    /// </summary>
    public class WebSocketLimit
    {
        /// <summary>
        /// WebSocket 的连接地址
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }
        /// <summary>
        /// 建议的 shard 数
        /// </summary>
        [JsonPropertyName("shards")]
        public int Shards { get; set; } = 1;
        /// <summary>
        /// 创建Session限制信息
        /// </summary>
        [JsonPropertyName("session_start_limit")]
        public SessionStartLimit? SessionLimit { get; set; }
    }

    /// <summary>
    /// Session开始限制
    /// </summary>
    public class SessionStartLimit
    {
        /// <summary>
        /// 每 24 小时可创建 Session 数
        /// </summary>
        [JsonPropertyName("total")]
        public int Total { get; set; }
        /// <summary>
        /// 目前还可以创建的 Session 数
        /// <para>每个机器人创建的连接数不能超过 remaining 剩余连接数</para>
        /// </summary>
        [JsonPropertyName("remaining")]
        public int Remaining { get; set; }
        /// <summary>
        /// 重置计数的剩余时间(ms)
        /// </summary>
        [JsonPropertyName("reset_after")]
        public int ResetAfter { get; set; }
        /// <summary>
        /// 每 5s 可以创建的 Session 数
        /// </summary>
        [JsonPropertyName("max_concurrency")]
        public int MaxConcurrency { get; set; }
    }
}
