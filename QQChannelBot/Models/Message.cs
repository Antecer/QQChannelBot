using System.Text.Json.Serialization;

namespace QQChannelBot.Models
{
    /// <summary>
    /// 消息对象
    /// </summary>
    public class Message
    {
        /// <summary>
        /// 消息id
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";
        /// <summary>
        /// 子频道 id
        /// </summary>
        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; } = "";
        /// <summary>
        /// 频道 id
        /// </summary>
        [JsonPropertyName("guild_id")]
        public string GuildId { get; set; } = "";
        /// <summary>
        /// 消息内容
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = "";
        /// <summary>
        /// 消息创建时间
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// 消息编辑时间
        /// </summary>
        [JsonPropertyName("edited_timestamp")]
        public DateTime EditedTimestamp { get; set; }
        /// <summary>
        /// 是否 @全员消息
        /// </summary>
        [JsonPropertyName("mention_everyone")]
        public bool MentionEveryone { get; set; }
        /// <summary>
        /// 消息创建者
        /// </summary>
        [JsonPropertyName("author")]
        public User Author { get; set; } = new();
        /// <summary>
        /// 附件(可多个)
        /// </summary>
        [JsonPropertyName("attachments")]
        public MessageAttachment[]? Attachments { get; set; }
        /// <summary>
        /// embed
        /// </summary>
        [JsonPropertyName("embeds")]
        public MessageEmbed[]? Embeds { get; set; }
        /// <summary>
        /// 消息中@的人
        /// </summary>
        [JsonPropertyName("mentions")]
        public List<User>? Mentions { get; set; }
        /// <summary>
        /// 消息创建者的member信息
        /// </summary>
        [JsonPropertyName("member")]
        public Member Member { get; set; } = new();
        /// <summary>
        /// ark消息
        /// </summary>
        [JsonPropertyName("ark")]
        public MessageArk? Ark { get; set; }
    }

    /// <summary>
    /// 消息体结构
    /// </summary>
    public class MessageToCreate
    {
        /// <summary>
        /// 构建消息体结构
        /// </summary>
        public MessageToCreate() { }
        /// <summary>
        /// 构建消息体结构
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="msgId">要回复的消息id，不填视为发送主动消息</param>
        public MessageToCreate(string content, string? msgId = null)
        {
            Content = content;
            MsgId = msgId;
        }
        /// <summary>
        /// 消息内容，文本内容，支持内嵌格式
        /// </summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; }
        /// <summary>
        /// embed 消息，一种特殊的 ark
        /// </summary>
        [JsonPropertyName("embed")]
        public MessageEmbed? Embed { get; set; }
        /// <summary>
        /// ark 消息
        /// </summary>
        [JsonPropertyName("ark")]
        public MessageArk? Ark { get; set; }
        /// <summary>
        /// 图片 url 地址
        /// </summary>
        [JsonPropertyName("image")]
        public string? Image { get; set; }
        /// <summary>
        /// 要回复的消息 id。带了 msg_id 视为被动回复消息，否则视为主动推送消息
        /// </summary>
        [JsonPropertyName("msg_id")]
        public string? MsgId { get; set; }
    }

    /// <summary>
    /// embed消息
    /// </summary>
    public class MessageEmbed
    {
        /// <summary>
        /// 标题
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        /// <summary>
        /// 描述 (见NodeSDK文档)
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        /// <summary>
        /// 消息弹窗内容
        /// </summary>
        [JsonPropertyName("prompt")]
        public string? Prompt { get; set; }
        /// <summary>
        /// 缩略图
        /// </summary>
        [JsonPropertyName("thumbnail")]
        public MessageEmbedThumbnail? Thumbnail { get; set; }
        /// <summary>
        /// 消息创建时间
        /// </summary>
        [JsonPropertyName("fields")]
        public MessageEmbedField[]? Fields { get; set; }
    }

    /// <summary>
    /// 缩略图对象
    /// </summary>
    public class MessageEmbedThumbnail
    {
        /// <summary>
        /// 图片地址
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }

    /// <summary>
    /// 消息创建时间对象
    /// </summary>
    public class MessageEmbedField
    {
        /// <summary>
        /// 字段名
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    /// <summary>
    /// 附件对象
    /// </summary>
    public class MessageAttachment
    {
        /// <summary>
        /// 下载地址
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { set; get; }
    }

    /// <summary>
    /// ark消息
    /// </summary>
    public class MessageArk
    {
        /// <summary>
        /// ark模板id（需要先申请）
        /// </summary>
        [JsonPropertyName("template_id")]
        public int TemplateId { get; set; }
        /// <summary>
        /// kv值列表
        /// </summary>
        [JsonPropertyName("kv")]
        public MessageArkKv[]? Kv { get; set; }
    }

    /// <summary>
    /// ark的键值对
    /// </summary>
    public class MessageArkKv
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }
        [JsonPropertyName("value")]
        public string? Value { get; set; }
        /// <summary>
        /// ark obj类型的列表
        /// </summary>
        [JsonPropertyName("obj")]
        public MessageArkObj[]? Obj { get; set; }
    }

    /// <summary>
    /// ark obj类型
    /// </summary>
    public class MessageArkObj
    {
        /// <summary>
        /// ark objkv列表
        /// </summary>
        [JsonPropertyName("obj_kv")]
        public MessageArkObjKv[]? ObjKv { get; set; }
    }

    /// <summary>
    /// ark obj键值对
    /// </summary>
    public class MessageArkObjKv
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }
        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }
}