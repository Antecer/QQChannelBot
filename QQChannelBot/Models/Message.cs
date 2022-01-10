using System.Text.Json.Serialization;
using QQChannelBot.Bot;

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
        [JsonPropertyName("timestamp"), JsonConverter(typeof(DateTimeToStringTimestamp))]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// 消息编辑时间
        /// </summary>
        [JsonPropertyName("edited_timestamp"), JsonConverter(typeof(DateTimeToStringTimestamp))]
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
        /// <summary>
        /// 存储BotClient对象，方便快速回复消息
        /// </summary>
        [JsonIgnore]
        public BotClient? Bot { get; set; }
        /// <summary>
        /// 快速回复
        /// <para>自动设置子频道Id和消息Id<br/>
        /// <em>使用快速回复会强制覆盖子频道Id和消息Id参数</em>
        /// </para>
        /// </summary>
        /// <param name="message">MessageToCreate消息对象(或其扩展对象)</param>
        /// <returns></returns>
        public async Task<Message?> ReplyAsync(MessageToCreate message)
        {
            message.MsgId = Id;
            return Bot != null ? await Bot.SendMessageAsync(ChannelId, message) : null;
        }
        /// <summary>
        /// 快速回复文字消息
        /// <para>自动添加子频道id参数</para>
        /// </summary>
        /// <param name="msg">文字消息内容</param>
        /// <returns></returns>
        public async Task<Message?> ReplyAsync(string msg)
        {
            return await ReplyAsync(new MessageToCreate() { Content = msg });
        }
    }

    /// <summary>
    /// 消息体结构
    /// </summary>
    public class MessageToCreate
    {
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
        /// 消息列表
        /// </summary>
        [JsonPropertyName("fields")]
        public List<MessageEmbedField>? Fields { get; set; }
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
    /// Embed行内容
    /// </summary>
    public class MessageEmbedField
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name"></param>
        public MessageEmbedField(string? name = null) { Name = name; }
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
        public List<MessageArkKv> Kv { get; set; } = new();
    }

    /// <summary>
    /// ark的键值对
    /// </summary>
    public class MessageArkKv
    {
        /// <summary>
        /// 键
        /// </summary>
        [JsonPropertyName("key")]
        public string Key { get; set; } = "";
        /// <summary>
        /// 值
        /// </summary>
        [JsonPropertyName("value")]
        public string? Value { get; set; }
        /// <summary>
        /// ark obj类型的列表
        /// </summary>
        [JsonPropertyName("obj")]
        public List<MessageArkObj>? Obj { get; set; }
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
        public List<MessageArkObjKv>? ObjKv { get; set; }
    }

    /// <summary>
    /// ark obj键值对
    /// </summary>
    public class MessageArkObjKv
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public MessageArkObjKv() { }
        /// <summary>
        /// ark obj键值对构造函数
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public MessageArkObjKv(string key, string value)
        {
            Key = key;
            Value = value;
        }
        /// <summary>
        /// 键
        /// </summary>
        [JsonPropertyName("key")]
        public string Key { get; set; } = "";
        /// <summary>
        /// 值
        /// </summary>
        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }
}