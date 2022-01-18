﻿using System.Text.Json.Serialization;

namespace QQChannelBot.Models
{
    /// <summary>
    /// 子频道权限对象
    /// </summary>
    public class ChannelPermissions
    {
        /// <summary>
        /// 子频道Id
        /// </summary>
        [JsonPropertyName("channel_id")]
        public string ChaannelId { get; set; } = "";
        /// <summary>
        /// 用户Id
        /// <para><em>此属性和RoleId只会同时存在一个</em></para>
        /// </summary>
        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }
        /// <summary>
        /// 身份组Id
        /// <para><em>此属性和UserId只会同时存在一个</em></para>
        /// </summary>
        [JsonPropertyName("role_id")]
        public string? RoleId { get; set; }
        /// <summary>
        /// 用户拥有的子频道权限
        /// <para>
        /// "1" - 查看 <br/>
        /// "2" - 管理 <br/>
        /// "4" - 发言
        /// </para>
        /// </summary>
        [JsonPropertyName("permissions"), JsonConverter(typeof(PrivacyTypeToStringNumberConverter))]
        public PrivacyType Permissions { get; set; }
    }

    /// <summary>
    /// 子频道私密权限
    /// </summary>
    [Flags]
    public enum PrivacyType
    {
        /// <summary>
        /// 没有任何权限
        /// </summary>
        隐藏 = 0,
        /// <summary>
        /// 可查看子频道	
        /// </summary>
        查看 = 1 << 0,
        /// <summary>
        /// 可管理子频道
        /// </summary>
        管理 = 1 << 1,
        /// <summary>
        /// 可发言子频道
        /// </summary>
        发言 = 1 << 2
    }
}
