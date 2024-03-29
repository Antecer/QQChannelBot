﻿using System.Text.Json.Serialization;

namespace QQChannelBot.Models
{
    /// <summary>
    /// 消息类型枚举
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// 公共
        /// </summary>
        Public,
        /// <summary>
        /// @机器人
        /// </summary>
        AtMe,
        /// <summary>
        /// @全员
        /// </summary>
        AtAll,
        /// <summary>
        /// 私聊
        /// </summary>
        Private
    }

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
        /// 是否 私聊消息
        /// </summary>
        [JsonPropertyName("direct_message")]
        public bool DirectMessage { get; set; }
        /// <summary>
        /// 是否 @全员消息
        /// </summary>
        [JsonPropertyName("mention_everyone")]
        public bool MentionEveryone { get; set; }
        /// <summary>
        /// 消息创建时间
        /// </summary>
        [JsonPropertyName("timestamp"), JsonConverter(typeof(DateTimeToStringTimestamp))]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 消息编辑时间
        /// </summary>
        [JsonPropertyName("edited_timestamp"), JsonConverter(typeof(DateTimeToStringTimestamp))]
        public DateTime EditedTime { get; set; }
        /// <summary>
        /// 消息创建者
        /// </summary>
        [JsonPropertyName("author")]
        public User Author { get; set; } = new();
        /// <summary>
        /// 附件(可多个)
        /// </summary>
        [JsonPropertyName("attachments")]
        public List<MessageAttachment>? Attachments { get; set; }
        /// <summary>
        /// embed
        /// </summary>
        [JsonPropertyName("embeds")]
        public List<MessageEmbed>? Embeds { get; set; }
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
        /// 引用消息（需要传递被引用的消息Id）
        /// </summary>
        [JsonPropertyName("message_reference")]
        public MessageReference? Reference { get; set; }
        /// <summary>
        /// 图片 url 地址
        /// </summary>
        [JsonPropertyName("image")]
        public string? Image { get; set; }
        /// <summary>
        /// 要回复的目标消息Id
        /// <para>带了 id 视为被动回复消息，否则视为主动推送消息</para>
        /// </summary>
        [JsonPropertyName("msg_id")]
        public string? Id { get; set; }
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
    /// 引用消息
    /// </summary>
    public class MessageReference
    {
        /// <summary>
        /// 需要引用回复的消息 id
        /// </summary>
        [JsonPropertyName("message_id")]
        public string? MessageId { get; set; }
        /// <summary>
        /// 是否忽略获取引用消息详情错误，默认否
        /// </summary>
        [JsonPropertyName("ignore_get_message_error")]
        public bool IgnoreGetMessageError { get; set; } = false;
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
        /// 附件Id
        /// </summary>
        [JsonPropertyName("id")]

        public string? Id { get; set; }
        /// <summary>
        /// 附件类型
        /// </summary>
        [JsonPropertyName("content_type")]
        public string? ContentType { get; set; }
        /// <summary>
        /// 下载地址
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { set; get; } = string.Empty;
        /// <summary>
        /// 文件名
        /// </summary>
        [JsonPropertyName("filename")]
        public string? FileName { get; set; }
        /// <summary>
        /// 附件大小
        /// </summary>
        [JsonPropertyName("size")]
        public long? Size { get; set; }
        /// <summary>
        /// 图片宽度
        /// <para>仅附件为图片时才有</para>
        /// </summary>
        [JsonPropertyName("width")]
        public int? Width { get; set; }
        /// <summary>
        /// 图片高度
        /// <para>仅附件为图片时才有</para>
        /// </summary>
        [JsonPropertyName("height")]
        public int? Height { get; set; }
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

    /// <summary>
    /// 拉取消息的操作类型
    /// </summary>
    public enum GetMsgTypesEnum
    {
        /// <summary>
        /// 获取目标id前后的消息
        /// </summary>
        around,
        /// <summary>
        /// 获取目标id之前的消息
        /// </summary>
        before,
        /// <summary>
        /// 获取目标id之后的消息
        /// </summary>
        after,
        /// <summary>
        /// 最新limit的消息
        /// </summary>
        latest
    }

    /// <summary>
    /// 消息审核对象
    /// </summary>
    public class MessageAudited
    {
        /// <summary>
        /// 消息审核Id
        /// </summary>
        [JsonPropertyName("audit_id")]
        public string AuditId { get; set; } = string.Empty;
        /// <summary>
        /// 被审核的消息Id
        /// <para>只有审核通过事件才会有值</para>
        /// </summary>
        [JsonPropertyName("message_id")]
        public string? MessageId { get; set; }
        /// <summary>
        /// 频道Id
        /// </summary>
        [JsonPropertyName("guild_id")]
        public string GuildId { get; set; } = string.Empty;
        /// <summary>
        /// 子频道Id
        /// </summary>
        [JsonPropertyName("channel_id")]
        public string ChannelId { get; set; } = string.Empty;
        /// <summary>
        /// 消息审核时间
        /// </summary>
        [JsonPropertyName("audit_time"), JsonConverter(typeof(DateTimeToStringTimestamp))]
        public DateTime AuditTime { get; set; }
        /// <summary>
        /// 消息创建时间
        /// </summary>
        [JsonPropertyName("create_time"), JsonConverter(typeof(DateTimeToStringTimestamp))]
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 扩展属性，用于标注审核是否通过
        /// </summary>
        [JsonIgnore]
        public bool IsPassed { get; set; } = false;
    }

    /// <summary>
    /// 私信会话对象（DMS）
    /// </summary>
    public record struct DirectMessageSource
    {
        /// <summary>
        /// 私信会话关联的频道Id
        /// </summary>
        [JsonPropertyName("guild_id")]
        public string GuildId { get; init; }
        /// <summary>
        /// 私信会话关联的子频道Id
        /// </summary>
        [JsonPropertyName("channel_id")]
        public string ChannelId { get; init; }
        /// <summary>
        /// 创建私信会话时间戳
        /// </summary>
        [JsonPropertyName("create_time"), JsonConverter(typeof(DateTimeToStringTimestamp))]
        public DateTime CreateTime { get; init; }
    }
}