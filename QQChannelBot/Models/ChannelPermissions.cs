using System.Text.Json.Serialization;

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
        /// "0" - 可查看子频道 <br/>
        /// "1" - 可管理子频道
        /// </para>
        /// </summary>
        [JsonPropertyName("permissions")]
        public string Permissions { get; set; } = "0";
    }
}
