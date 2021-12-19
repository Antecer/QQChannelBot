using System.Text.Json.Serialization;

namespace QQChannelBot.Models
{
    /// <summary>
    /// 成员
    /// </summary>
    public class Member
    {
        /// <summary>
        /// 用户基础信息，来自QQ资料，只有成员相关接口中会填充此信息
        /// </summary>
        [JsonPropertyName("user")]
        public User? User { get; set; }
        /// <summary>
        /// 用户在频道内的昵称(默认为空)
        /// </summary>
        [JsonPropertyName("nick")]
        public string? Nick { get; set; }
        /// <summary>
        /// 用户在频道内的身份组ID, 默认值可参考DefaultRoles
        /// </summary>
        [JsonPropertyName("roles")]
        public string[] Roles { get; set; } = new string[] { "1" };
        /// <summary>
        /// 用户加入频道的时间 ISO8601 timestamp
        /// </summary>
        [JsonPropertyName("joined_at")]
        public string JoinedAt { get; set; } = "null";
    }

    /// <summary>
    /// 有频道ID的成员
    /// </summary>
    public class MemberWithGuildID : Member
    {
        /// <summary>
        /// 频道id
        /// </summary>
        [JsonPropertyName("guild_id")]
        public string? GuildId { get; set; }
    }
}
