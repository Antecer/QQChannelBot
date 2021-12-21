using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using QQChannelBot.Models;

namespace QQChannelBot.BotApi
{
    /// <summary>
    /// 频道身份组列表
    /// </summary>
    public class GuildRoles
    {
        /// <summary>
        /// 频道Id
        /// </summary>
        [JsonPropertyName("guild_id")]
        public string GuildId { get; set; } = "";

        /// <summary>
        /// 身份组
        /// </summary>
        [JsonPropertyName("roles")]
        public Role[] Roles { get; set; } = Array.Empty<Role>();
        /// <summary>
        /// 默认分组上限
        /// </summary>
        [JsonPropertyName("role_num_limit")]
        public string RoleNumLimit { get; set; } = "";
    }

    /// <summary>
    /// 新建频道身份组的返回值
    /// </summary>
    public class CreateRoleRes
    {

        /// <summary>
        /// 身份组ID
        /// </summary>
        [JsonPropertyName("role_id")]
        public string? RoleId { get; set; }
        /// <summary>
        /// 新创建的频道身份组对象
        /// </summary>
        [JsonPropertyName("role")]
        public Role? Role { get; set; }
    }

    /// <summary>
    /// 修改频道身份组的返回值
    /// </summary>
    public class ModifyRolesRes
    {
        /// <summary>
        /// 身份组ID
        /// </summary>
        [JsonPropertyName("guild_id")]
        public string? GuildId { get; set; }
        /// <summary>
        /// 身份组ID
        /// </summary>
        [JsonPropertyName("role_id")]
        public string? RoleId { get; set; }
        /// <summary>
        /// 新创建的频道身份组对象
        /// </summary>
        [JsonPropertyName("role")]
        public Role? Role { get; set; }
    }

    /// <summary>
    /// 标识需要设置哪些字段
    /// </summary>
    public class Filter
    {
        /// <summary>
        /// 是否设置名称: 0-否, 1-是
        /// </summary>
        [JsonPropertyName("name")]
        public int Name { get; set; }
        /// <summary>
        /// 是否设置颜色: 0-否, 1-是
        /// </summary>
        [JsonPropertyName("color")]
        public int Color { get; set; }
        /// <summary>
        /// 是否设置在成员列表中单独展示: 0-否, 1-是
        /// </summary>
        [JsonPropertyName("hoist")]
        public int Hoist { get; set; }
    }

    /// <summary>
    /// 携带需要设置的字段内容
    /// </summary>
    public class Info
    {
        /// <summary>
        /// 名称
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        /// <summary>
        /// ARGB的HEX十六进制颜色值转换后的十进制数值
        /// </summary>
        [JsonPropertyName("color")]
        private uint? Color { get; set; }
        /// <summary>
        /// ARGB的HTML十六进制颜色值
        /// <para>支持这些格式：#FFFFFFFF #FFFFFF #FFFF #FFF</para>
        /// <para><em>注: 因官方API有BUG，框架暂时强制Alpha通道固定为1.0 [2021-12-21]</em></para>
        /// </summary>
        [JsonIgnore]
        public string? HexColor
        {
            get
            {
                return Color != null ? $"#{Color:X8}" : null;
            }
            set
            {
                value = value?.Trim('#') ?? "#000";
                Color = value.Length switch
                {
                    3 => Convert.ToUInt32($"{value[0]}{value[0]}{value[1]}{value[1]}{value[2]}{value[2]}", 16),
                    4 => Convert.ToUInt32($"{value[0]}{value[0]}{value[1]}{value[1]}{value[2]}{value[2]}{value[3]}{value[3]}", 16),
                    6 => Convert.ToUInt32(value, 16),
                    8 => Convert.ToUInt32(value, 16),
                    _ => 0x0
                } | 0xFF000000;
            }
        }
        /// <summary>
        /// 在成员列表中单独展示: 0-否, 1-是
        /// </summary>
        [JsonPropertyName("hoist")]
        public int? Hoist { get; set; }
    }
}
