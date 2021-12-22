using System.Text.Json.Serialization;

namespace QQChannelBot.Models
{
    /// <summary>
    /// 频道对象
    /// </summary>
    public class Guild
    {
        /// <summary>
        /// 频道ID
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        /// <summary>
        /// 频道名称
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        /// <summary>
        /// 频道头像地址
        /// </summary>
        [JsonPropertyName("icon")]
        public string? Icon { get; set; }
        /// <summary>
        /// 频道创建人用户ID
        /// </summary>
        [JsonPropertyName("owner_id")]
        public string? OwnerId { get; set; }
        /// <summary>
        /// 当前人是否是频道创建人
        /// </summary>
        [JsonPropertyName("owner")]
        public bool Owner { get; set; }
        /// <summary>
        /// 成员数
        /// </summary>
        [JsonPropertyName("member_count")]
        public int MemberCount { get; set; }
        /// <summary>
        /// 最大成员数
        /// </summary>
        [JsonPropertyName("max_members")]
        public int MaxMembers { get; set; }
        /// <summary>
        /// 频道描述
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        /// <summary>
        /// 加入时间
        /// </summary>
        [JsonPropertyName("joined_at")]
        public string? JoinedAt { get; set; }
    }

    /// <summary>
    /// 禁言模式
    /// </summary>
    public class MuteMode
    {
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
