﻿using System.Text.Json.Serialization;

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
        public string Id { get; set; } = string.Empty;
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
        /// 频道创建时间
        /// </summary>
        [JsonPropertyName("joined_at"), JsonConverter(typeof(DateTimeToStringTimestamp))]
        public DateTime JoinedAt { get; set; }
        /// <summary>
        /// 机器人在本频道内拥有的权限的列表
        /// </summary>
        [JsonIgnore]
        public List<APIPermission>? APIPermissions { get; set; }
    }

    /// <summary>
    /// 频道信息
    /// </summary>
    public class GuildInfo : Guild
    {
        /// <summary>
        /// 子频道列表
        /// </summary>
        public HashSet<Channel> Channels { get; set; } = new();
        /// <summary>
        /// 角色列表
        /// </summary>
        public HashSet<Role> Roles { get; set; } = new();
        /// <summary>
        /// 成员列表
        /// </summary>
        public HashSet<Member> Members { get; set; } = new();
    }
}
