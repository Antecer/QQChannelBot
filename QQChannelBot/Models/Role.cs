using System.Drawing;
using System.Text.Json.Serialization;

namespace QQChannelBot.Models
{
    /// <summary>
    /// 身分组对象
    /// </summary>
    public class Role
    {
        /// <summary>
        /// 身份组ID, 默认值可参考 <see cref="DefaultRoles"/>
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// 身分组名称
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 身份组颜色
        /// </summary>
        [JsonPropertyName("color"), JsonConverter(typeof(ColorToUint32Converter))]
        public Color Color { get; set; }
        /// <summary>
        /// ARGB颜色值的HTML表现形式（如：#FFFFFFFF）
        /// </summary>
        [JsonIgnore]
        public string? ColorHtml { get => $"#{Color.ToArgb():X8}"; }
        /// <summary>
        /// 该身分组是否在成员列表中单独展示
        /// </summary>
        [JsonPropertyName("hoist"), JsonConverter(typeof(BoolToInt32Converter))]
        public bool Hoist { get; set; }
        /// <summary>
        /// 该身分组的人数
        /// </summary>
        [JsonPropertyName("number")]
        public uint Number { get; set; }
        /// <summary>
        /// 成员上限
        /// </summary>
        [JsonPropertyName("member_limit")]
        public uint MemberLimit { get; set; }
    }
    /// <summary>
    /// 频道身份组列表
    /// </summary>
    public class GuildRoles
    {
        /// <summary>
        /// 频道Id
        /// </summary>
        [JsonPropertyName("guild_id")]
        public string? GuildId { get; set; }

        /// <summary>
        /// 身份组
        /// </summary>
        [JsonPropertyName("roles")]
        public List<Role>? Roles { get; set; }
        /// <summary>
        /// 默认分组上限
        /// </summary>
        [JsonPropertyName("role_num_limit")]
        public string? RoleNumLimit { get; set; }
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
        /// 配置筛选器
        /// </summary>
        /// <param name="setName">设置名称</param>
        /// <param name="setColor">设置颜色</param>
        /// <param name="setHoist">设置在成员列表中单独展示</param>
        public Filter(bool setName = false, bool setColor = false, bool setHoist = false)
        {
            Name = setName;
            Color = setColor;
            Hoist = setHoist;
        }
        /// <summary>
        /// 是否设置名称
        /// </summary>
        [JsonPropertyName("name"), JsonConverter(typeof(BoolToInt32Converter))]
        public bool Name { get; set; }
        /// <summary>
        /// 是否设置颜色
        /// </summary>
        [JsonPropertyName("color"), JsonConverter(typeof(BoolToInt32Converter))]
        public bool Color { get; set; }
        /// <summary>
        /// 是否设置在成员列表中单独展示
        /// </summary>
        [JsonPropertyName("hoist"), JsonConverter(typeof(BoolToInt32Converter))]
        public bool Hoist { get; set; }
    }
    /// <summary>
    /// 携带需要设置的字段内容
    /// </summary>
    public class Info
    {
        /// <summary>
        /// 构造身份组信息
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="color">颜色</param>
        /// <param name="hoist">在成员列表中单独展示</param>
        public Info(string? name = null, Color? color = null, bool? hoist = null)
        {
            Name = name;
            Color = color;
            Hoist = hoist;
        }
        /// <summary>
        /// 构造身份组信息
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="colorHtml">ARGB颜色值的HTML表现形式（如：#FFFFFFFF）</param>
        /// <param name="hoist">在成员列表中单独展示</param>
        public Info(string? name = null, string? colorHtml = null, bool? hoist = null)
        {
            Name = name;
            ColorHtml = colorHtml;
            Hoist = hoist;
        }
        /// <summary>
        /// 名称
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        /// <summary>
        /// 颜色
        /// </summary>
        [JsonPropertyName("color"), JsonConverter(typeof(ColorToUint32Converter))]
        public Color? Color { get; set; }
        /// <summary>
        /// ARGB的HTML十六进制颜色值
        /// <para>
        /// 支持这些格式(忽略大小写和前后空白字符)：<br/>
        /// #FFFFFFFF #FFFFFF #FFFF #FFF
        /// </para>
        /// <para><em>注: 因官方API有BUG，框架暂时强制Alpha通道固定为1.0，对功能无影响。 [2021-12-21]</em></para>
        /// </summary>
        [JsonIgnore]
        public string? ColorHtml
        {
            get
            {
                return Color == null ? null : $"#{(uint)Color.Value.ToArgb():X8}";
            }
            set
            {
                value = value?.TrimStart("0x", "#");
                if (value?.Length == 5) value = $"#{value[1]}{value[1]}{value[2]}{value[2]}{value[3]}{value[3]}{value[4]}{value[4]}";
                if (string.IsNullOrWhiteSpace(value)) Color = null;
                else Color = ColorTranslator.FromHtml(value);
            }
        }
        /// <summary>
        /// 在成员列表中单独展示
        /// </summary>
        [JsonPropertyName("hoist"), JsonConverter(typeof(BoolToInt32Converter))]
        public bool? Hoist { get; set; }
    }
    /// <summary>
    /// 系统默认身份组
    /// </summary>
    public static class DefaultRoles
    {
        /// <summary>
        /// 获取系统默认身份组名称
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public static string? Get(string roleId)
        {
            return roleId switch
            {
                "1" => "普通成员",
                "2" => "管理员",
                "4" => "频道主",
                "5" => "子频道管理员",
                _ => null
            };
        }
    }
}
