using System.Net.Http.Json;
using QQChannelBot.Models;

namespace QQChannelBot.Bot
{
    public partial class BotClient
    {
        /// <summary>
        /// 获取频道身份组列表
        /// </summary>
        /// <param name="guild_id">频道Id</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<List<Role>?> GetRolesAsync(string guild_id, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/guilds/{guild_id}/roles", null, null, sender);
            var result = respone == null ? null : await respone.Content.ReadFromJsonAsync<GuildRoles?>();
            return result?.Roles;
        }
        /// <summary>
        /// 创建频道身份组
        /// </summary>
        /// <param name="guild_id">频道Id</param>
        /// <param name="info">携带需要设置的字段内容</param>
        /// <param name="filter">标识需要设置哪些字段,若不填则根据Info自动推测</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<Role?> CreateRoleAsync(string guild_id, Info info, Filter? filter = null, Sender? sender = null)
        {
            filter ??= new Filter(!string.IsNullOrWhiteSpace(info.Name), info.Color != null, info.Hoist ?? false);
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/guilds/{guild_id}/roles", HttpMethod.Post, JsonContent.Create(new { filter, info }), sender);
            var result = respone == null ? null : await respone.Content.ReadFromJsonAsync<CreateRoleRes?>();
            return result?.Role;

        }
        /// <summary>
        /// 修改频道身份组
        /// </summary>
        /// <param name="guild_id">频道Id</param>
        /// <param name="role_id">角色Id</param>
        /// <param name="info">携带需要修改的字段内容</param>
        /// <param name="filter">标识需要设置哪些字段,若不填则根据Info自动推测</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<Role?> EditRoleAsync(string guild_id, string role_id, Info info, Filter? filter = null, Sender? sender = null)
        {
            filter ??= new Filter(!string.IsNullOrWhiteSpace(info.Name), info.Color != null, info.Hoist ?? false);
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/guilds/{guild_id}/roles/{role_id}", HttpMethod.Patch, JsonContent.Create(new { filter, info }), sender);
            var result = respone == null ? null : await respone.Content.ReadFromJsonAsync<ModifyRolesRes?>();
            return result?.Role;
        }
        /// <summary>
        /// 删除身份组
        /// <para><em>HTTP状态码 204 表示成功</em></para>
        /// </summary>
        /// <param name="guild_id">频道Id</param>
        /// <param name="role_id">身份Id</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<bool> DeleteRoleAsync(string guild_id, string role_id, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/guilds/{guild_id}/roles/{role_id}", HttpMethod.Delete, null, sender);
            return respone?.IsSuccessStatusCode ?? false;
        }
        /// <summary>
        /// 增加频道身份组成员
        /// <para>
        /// 需要使用的 token 对应的用户具备增加身份组成员权限。如果是机器人，要求被添加为管理员。 <br/>
        /// 如果要增加的身份组ID是(5-子频道管理员)，需要增加 channel_id 来指定具体是哪个子频道。
        /// </para>
        /// </summary>
        /// <param name="guild_id">频道Id</param>
        /// <param name="user_id">用户Id</param>
        /// <param name="role_id">身份组Id</param>
        /// <param name="channel_id">子频道Id</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<bool> AddRoleMemberAsync(string guild_id, string user_id, string role_id, string? channel_id = null, Sender? sender = null)
        {
            HttpContent? httpContent = channel_id == null ? null : JsonContent.Create(new { channel = new Channel { Id = channel_id } });
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/guilds/{guild_id}/members/{user_id}/roles/{role_id}", HttpMethod.Put, httpContent, sender);
            return respone?.IsSuccessStatusCode ?? false;
        }
        /// <summary>
        /// 删除频道身份组成员
        /// <para>
        /// 需要使用的 token 对应的用户具备删除身份组成员权限。如果是机器人，要求被添加为管理员。 <br/>
        /// 如果要删除的身份组ID是(5-子频道管理员)，需要设置 channel_id 来指定具体是哪个子频道。 <br/>
        /// 详情查阅 <see href="https://bot.q.qq.com/wiki/develop/api/openapi/guild/delete_guild_member_role.html">QQ机器人文档</see>
        /// </para>
        /// </summary>
        /// <param name="guild_id">频道Id</param>
        /// <param name="user_id">要加入身份组的用户Id</param>
        /// <param name="role_id">身份组Id</param>
        /// <param name="channel_id">子频道Id</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<bool> DeleteRoleMemberAsync(string guild_id, string user_id, string role_id, string? channel_id = null, Sender? sender = null)
        {
            HttpContent? httpContent = channel_id == null ? null : JsonContent.Create(new { channel = new Channel { Id = channel_id } });
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/guilds/{guild_id}/members/{user_id}/roles/{role_id}", HttpMethod.Delete, httpContent, sender);
            return respone?.IsSuccessStatusCode ?? false;
        }
    }
}
