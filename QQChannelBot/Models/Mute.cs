using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace QQChannelBot.Models
{
    /// <summary>
    /// 禁言时间
    /// </summary>
    public class MuteTime
    {
        public MuteTime()
        {
            MuteSeconds = "60";
        }
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
        /// <param name="timestamp">解禁时间戳
        /// <para>
        /// 格式："yyyy-MM-dd HH:mm:ss"<br/>
        /// 示例："2077-01-01 08:00:00"
        /// </para></param>
        public MuteTime(string timestamp)
        {
            MuteEndTimestamp = new DateTimeOffset(Convert.ToDateTime(timestamp)).ToUnixTimeSeconds().ToString();
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

    /// <summary>
    /// 根据时间字符串构建禁言时间
    /// </summary>
    public class MuteMaker : MuteTime
    {
        readonly Regex TypeTimeStamp = new(@"(\d{4})[-年](\d\d)[-月](\d\d)[\s日]*(\d\d)[:点时](\d\d)[:分](\d\d)秒?");
        readonly Regex TypeTimeDelay = new(@"(\d+)\s*(年|星期|周|日|天|小?时|分钟?|秒钟?)");
        /// <summary>
        /// 构造禁言时间
        /// <para>
        /// 支持以下正则匹配的格式 (优先使用时间戳模式)：<br/>
        /// 时间戳模式 - "^(\d{4})[-年](\d\d)[-月](\d\d)[\s日]*(\d\d)[:点时](\d\d)[:分](\d\d)秒?\s*$"<br/>
        /// 倒计时模式 - "^(\d+)\s*(年|星期|周|日|天|小?时|分钟?|秒钟?)?\s*$"
        /// </para>
        /// </summary>
        public MuteMaker(string timeString = "1分钟")
        {
            if (string.IsNullOrEmpty(timeString)) return;
            Match mts = TypeTimeStamp.Match(timeString);
            if (mts.Success)
            {
                string timstamp = $"{mts.Groups[1].Value}-{mts.Groups[2].Value}-{mts.Groups[3].Value} {mts.Groups[4].Value}:{mts.Groups[5].Value}:{mts.Groups[6].Value}";
                MuteEndTimestamp = new DateTimeOffset(Convert.ToDateTime(timstamp)).ToUnixTimeSeconds().ToString();
            }
            else
            {
                Match mtd = TypeTimeDelay.Match(timeString);
                if (mtd.Success)
                {
                    int seconds = mtd.Groups[2].Value switch
                    {
                        "年" => 60 * 60 * 24 * 365,
                        "星期" => 60 * 60 * 24 * 7,
                        "周" => 60 * 60 * 24 * 7,
                        "日" => 60 * 60 * 24,
                        "天" => 60 * 60 * 24,
                        "小时" => 60 * 60,
                        "时" => 60 * 60,
                        "分钟" => 60,
                        "分" => 60,
                        _ => 1
                    } * int.Parse(mtd.Groups[1].Value);
                    MuteSeconds = seconds.ToString();
                }
            }
        }
    }
}
