using System.Text.Json.Serialization;

namespace QQChannelBot.Models
{
    /// <summary>
    /// 接口权限对象
    /// </summary>
    public class APIPermission
    {
        /// <summary>
        /// 接口地址
        /// <para>
        /// 例：/guilds/{guild_id}/members/{user_id}
        /// </para>
        /// </summary>
        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;
        /// <summary>
        /// 请求方法，例：GET
        /// </summary>
        [JsonPropertyName("method")]
        public string Method { get; set; } = "GET";
        /// <summary>
        /// API 接口名称，例：获取频道信息
        /// </summary>
        [JsonPropertyName("desc")]
        public string Desc { get; set; } = string.Empty;
        /// <summary>
        /// 授权状态
        /// <para>
        /// 0 - 未授权<br/>
        /// 1 - 已授权
        /// </para>
        /// </summary>
        [JsonPropertyName("auth_status")]
        public int AuthStatus { get; set; } = 0;
    }
    /// <summary>
    /// 接口权限列表对象
    /// </summary>
    public class APIPermissions
    {
        /// <summary>
        /// 接口权限列表
        /// </summary>
        [JsonPropertyName("apis")]
        public List<APIPermission>? List { get; set; }
    }

    /// <summary>
    /// 接口权限需求对象
    /// </summary>
    public class APIPermissionDemand
    {
        /// <summary>
        /// 申请接口权限的频道 id
        /// </summary>
        [JsonPropertyName("guild_id")]
        public string? GuildId { get; set; }
        /// <summary>
        /// 接口权限需求授权链接发送的子频道 id
        /// </summary>
        [JsonPropertyName("channel_id")]
        public string? ChannelId { get; set; }
        /// <summary>
        /// 权限接口唯一标识
        /// </summary>
        [JsonPropertyName("api_identify")]
        public APIPermissionDemandIdentify ApiIdentify { get; set; }
        /// <summary>
        /// 接口权限链接中的接口权限描述信息
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// 接口权限链接中的机器人可使用功能的描述信息
        /// </summary>
        [JsonPropertyName("desc")]
        public string Desc { get; set; } = string.Empty;
    }
    /// <summary>
    /// 接口权限需求标识对象
    /// </summary>
    public record struct APIPermissionDemandIdentify
    {
        /// <summary>
        /// 接口地址
        /// <para>
        /// 例：/guilds/{guild_id}/members/{user_id}
        /// </para>
        /// </summary>
        [JsonPropertyName("path")]
        public string Path { get; init; }
        /// <summary>
        /// 请求方法，例：GET
        /// </summary>
        [JsonPropertyName("method")]
        public string Method { get; init; }
    }
}
