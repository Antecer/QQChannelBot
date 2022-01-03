using System.Text.Json.Serialization;

namespace QQChannelBot.Models
{
    /// <summary>
    /// 身分组对象
    /// </summary>
    public class Role
    {
        /// <summary>
        /// 身份组ID, 默认值可参考 DefaultRoles
        /// <para>
        /// "1" - 全体成员 <br/>
        /// "2" - 管理员 <br/>
        /// "3" - 群主/创建者 <br/>
        /// "4" - 子频道管理员
        /// </para>
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// 身分组名称
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// ARGB的HEX十六进制颜色值转换后的十进制数值
        /// </summary>
        [JsonPropertyName("color")]
        public uint Color { get; set; }
        /// <summary>
        /// 该身分组是否在成员列表中单独展示: 0-否, 1-是
        /// </summary>
        [JsonPropertyName("hoist")]
        public uint Hoist { get; set; }
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
    /// 系统默认身份组
    /// </summary>
    public class DefaultRoles
    {
        /// <summary>
        /// 获取系统默认身份组名称
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public static string GetName(string roleId)
        {
            return roleId switch
            {
                "1" => "普通成员",
                "2" => "管理员",
                "3" => "频道主",
                "4" => "子频道管理员",
                _ => "未知角色"
            };
        }
    }
}
