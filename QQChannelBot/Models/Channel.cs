using System.Text.Json.Serialization;

namespace QQChannelBot.Models
{
    /// <summary>
    /// 子频道对象
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// 子频道id
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
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
        /// 子频道类型
        /// </summary>
        [JsonPropertyName("type")]
        public ChannelType Type { get; set; }
        /// <summary>
        /// 子频道子类型
        /// </summary>
        [JsonPropertyName("sub_type")]
        public ChannelSubType SubType { get; set; }
        /// <summary>
        /// 频道位置排序，非必填，但不能够和其他子频道的值重复
        /// </summary>
        [JsonPropertyName("position")]
        public int? Possition { get; set; }
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
        /// <summary>
        /// 子频道私密类型
        /// </summary>
        [JsonPropertyName("private_type")]

        public ChannelPrivateType PrivateType { get; set; }
    }
    /// <summary>
    /// 子频道类型
    /// </summary>
    public enum ChannelType
    {
        /// <summary>
        /// 文字子频道
        /// </summary>
        文字 = 0,
        /// <summary>
        /// 保留，不可用
        /// </summary>
        Reserve1 = 1,
        /// <summary>
        /// 语音子频道
        /// </summary>
        语音 = 2,
        /// <summary>
        /// 保留，不可用
        /// </summary>
        Reserve2 = 3,
        /// <summary>
        /// 子频道分组
        /// </summary>
        分组 = 4,
        /// <summary>
        /// 直播子频道
        /// </summary>
        直播 = 10005,
        /// <summary>
        /// 应用子频道
        /// </summary>
        应用 = 10006,
        /// <summary>
        /// 论坛子频道
        /// </summary>
        论坛 = 10007
    }
    /// <summary>
    /// 子频道子类型(目前只有文字子频道有)
    /// </summary>
    public enum ChannelSubType
    {
        /// <summary>
        /// 闲聊
        /// </summary>
        闲聊,
        /// <summary>
        /// 公告
        /// </summary>
        公告,
        /// <summary>
        /// 攻略
        /// </summary>
        攻略,
        /// <summary>
        /// 开黑
        /// </summary>
        开黑
    }
    /// <summary>
    /// 子频道私密类型
    /// </summary>
    public enum ChannelPrivateType
    {
        /// <summary>
        /// 公开频道
        /// </summary>
        Public,
        /// <summary>
        /// 群主和管理员可见
        /// </summary>
        Admin,
        /// <summary>
        /// 群主和管理员+指定成员可见
        /// </summary>
        Members
    }
}
