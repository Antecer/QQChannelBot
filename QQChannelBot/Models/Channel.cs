using System.Text.Json.Serialization;

namespace QQChannelBot.Models
{
    /// <summary>
    /// 子频道类型
    /// </summary>
    public enum ChannelType
    {
        /// <summary>
        /// 文字子频道
        /// </summary>
        TextChannel = 0,
        /// <summary>
        /// 保留，不可用
        /// </summary>
        Reserve1 = 1,
        /// <summary>
        /// 语音子频道
        /// </summary>
        VoiceChannel = 2,
        /// <summary>
        /// 保留，不可用
        /// </summary>
        Reserve2 = 3,
        /// <summary>
        /// 子频道分组
        /// </summary>
        ChannelSubGroup = 4,
        /// <summary>
        /// 直播子频道
        /// </summary>
        LiveChannel = 10005,
        /// <summary>
        /// 应用子频道
        /// </summary>
        AppChannel = 10006,
        /// <summary>
        /// 论坛子频道
        /// </summary>
        ForumChannel = 10007
    }

    /// <summary>
    /// 子频道子类型(目前只有文字子频道有)
    /// </summary>
    public enum ChannelSubType
    {
        /// <summary>
        /// 闲聊
        /// </summary>
        General,
        /// <summary>
        /// 公告
        /// </summary>
        Declared,
        /// <summary>
        /// 攻略
        /// </summary>
        Raiders,
        /// <summary>
        /// 开黑
        /// </summary>
        GangUp,
    }

    /// <summary>
    /// 子频道对象
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// 子频道id
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        /// <summary>
        /// 频道id
        /// </summary>
        [JsonPropertyName("guild_id")]
        public string? GuildId { get; set; }
        /// <summary>
        /// 子频道名称
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        /// <summary>
        /// 子频道类型 ChannelType
        /// </summary>
        [JsonPropertyName("type")]
        public ChannelType Type { get; set; }
        /// <summary>
        /// 子频道子类型 ChannelSubType
        /// </summary>
        [JsonPropertyName("sub_type")]
        public ChannelSubType SubType { get; set; }
        /// <summary>
        /// 频道位置排序，必填，而且不能够和其他子频道的值重复
        /// </summary>
        [JsonPropertyName("position")]
        public int Possition { get; set; }
        /// <summary>
        /// 分组 id
        /// </summary>
        [JsonPropertyName("parent_id")]
        public string? ParentId { get; set; }
        /// <summary>
        /// 创建人 id
        /// </summary>
        [JsonPropertyName("owner_id")]
        public string? OwerId { get; set; }
    }
}
