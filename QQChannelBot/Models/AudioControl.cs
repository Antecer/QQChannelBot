using System.Text.Json.Serialization;

namespace QQChannelBot.Models
{
    /// <summary>
    /// 音频控制
    /// </summary>
    public class AudioControl
    {
        /// <summary>
        /// 音频数据的url status为0时传
        /// </summary>
        [JsonPropertyName("audio_url")]
        public string? AudioUrl { get; set; }
        /// <summary>
        /// 状态文本（比如：简单爱-周杰伦），可选，status为0时传，其他操作不传
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }
        /// <summary>
        /// 播放状态，参考 STATUS
        /// </summary>
        [JsonPropertyName("status")]
        public STATUS Status { get; set; }
    }
    /// <summary>
    /// 枚举播放状态
    /// </summary>
    public enum STATUS
    {
        /// <summary>
        /// 开始播放操作
        /// </summary>
        START,
        /// <summary>
        /// 暂停播放操作
        /// </summary>
        PAUSE,
        /// <summary>
        /// 继续播放操作
        /// </summary>
        RESUME,
        /// <summary>
        /// 停止播放操作
        /// </summary>
        STOP
    }
    /// <summary>
    /// 语音Action
    /// </summary>
    public class AudioAction
    {
        /// <summary>
        /// 频道id
        /// </summary>
        [JsonPropertyName("guild_id")]
        public string? GuildId { get; set; }
        /// <summary>
        /// 子频道id
        /// </summary>
        [JsonPropertyName("channel_id")]
        public string? ChannelId { get; set; }
        /// <summary>
        /// 音频数据的url status为0时传
        /// </summary>
        [JsonPropertyName("audio_url")]
        public string? AudioUrl { get; set; }
        /// <summary>
        /// 状态文本（比如：简单爱-周杰伦），可选，status为0时传，其他操作不传
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { set; get; }
    }
}
