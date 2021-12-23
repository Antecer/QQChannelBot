using System.Text.Json.Serialization;

namespace QQChannelBot.Models
{
    /// <summary>
    /// 禁言时间
    /// </summary>
    public class MuteTime
    {
        /// <summary>
        /// 禁言指定的时长
        /// </summary>
        /// <param name="seconds">禁言多少秒</param>
        public MuteTime(int seconds)
        {
            MuteSeconds = seconds.ToString();
        }
        /// <summary>
        /// 禁言到指定时间
        /// </summary>
        /// <param name="timstamp">解禁时间戳
        /// <para>
        /// 格式："yyyy-MM-dd HH:mm:ss"<br/>
        /// 示例："2077-01-01 08:00:00"
        /// </para></param>
        public MuteTime(string timstamp)
        {
            MuteEndTimestamp = new DateTimeOffset(Convert.ToDateTime(timstamp)).ToUnixTimeSeconds().ToString();
        }
        /// <summary>
        /// 禁言到期时间戳，绝对时间戳，单位：秒
        /// <para><em>这里单词timestamp拼写错误，等待官方修复 [2021年12月22日]</em></para>
        /// </summary>
        [JsonPropertyName("mute_end_timstamp")]
        public string? MuteEndTimestamp { get; set; }
        /// <summary>
        /// 禁言多少秒
        /// </summary>
        [JsonPropertyName("mute_seconds")]
        public string? MuteSeconds { get; set; }
    }
}
