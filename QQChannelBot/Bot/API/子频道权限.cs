using System.Net.Http.Json;
using QQChannelBot.Models;

namespace QQChannelBot.Bot
{
    public partial class BotClient
    {
        /// <summary>
        /// 获取用户在指定子频道的权限
        /// </summary>
        /// <param name="channel_id">子频道Id</param>
        /// <param name="user_id">用户Id</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<ChannelPermissions?> GetChannelPermissionsAsync(string channel_id, string user_id, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/members/{user_id}/permissions", null, null, sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<ChannelPermissions?>();
        }
        /// <summary>
        /// 修改用户在指定子频道的权限
        /// </summary>
        /// <param name="channel_id">子频道Id</param>
        /// <param name="user_id">用户Id</param>
        /// <param name="add">添加的权限</param>
        /// <param name="remove">删除的权限</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<bool> EditChannelPermissionsAsync(string channel_id, string user_id, string add = "0", string remove = "0", Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/members/{user_id}/permissions", HttpMethod.Put, JsonContent.Create(new
            {
                add,
                remove
            }), sender);
            return respone?.IsSuccessStatusCode ?? false;
        }
        /// <summary>
        /// 修改用户在指定子频道的权限
        /// </summary>
        /// <param name="channel_id">子频道Id</param>
        /// <param name="user_id">用户Id</param>
        /// <param name="permission">修改后的权限</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<bool> EditChannelPermissionsAsync(string channel_id, string user_id, PrivacyType permission, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/members/{user_id}/permissions", HttpMethod.Put, JsonContent.Create(new
            {
                remove = (~permission.GetHashCode()).ToString(),
                add = permission.GetHashCode().ToString()
            }), sender);
            return respone?.IsSuccessStatusCode ?? false;
        }
        /// <summary>
        /// 获取指定身份组在指定子频道的权限
        /// </summary>
        /// <param name="channel_id">子频道Id</param>
        /// <param name="role_id">身份组Id</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<ChannelPermissions?> GetMemberChannelPermissionsAsync(string channel_id, string role_id, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/roles/{role_id}/permissions", null, null, sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<ChannelPermissions?>();
        }
        /// <summary>
        /// 修改指定身份组在指定子频道的权限
        /// </summary>
        /// <param name="channel_id">子频道Id</param>
        /// <param name="role_id">身份组Id</param>
        /// <param name="add">添加的权限</param>
        /// <param name="remove">删除的权限</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<bool> EditMemberChannelPermissionsAsync(string channel_id, string role_id, string add = "0", string remove = "0", Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/roles/{role_id}/permissions", HttpMethod.Put, JsonContent.Create(new
            {
                add,
                remove
            }), sender);
            return respone?.IsSuccessStatusCode ?? false;
        }
        /// <summary>
        /// 修改指定身份组在指定子频道的权限
        /// <para>注：本接口不支持修改 "可管理子频道" 权限</para>
        /// </summary>
        /// <param name="channel_id">子频道Id</param>
        /// <param name="role_id">身份组Id</param>
        /// <param name="permission">修改后的权限</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<bool> EditMemberChannelPermissionsAsync(string channel_id, string role_id, PrivacyType permission, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/roles/{role_id}/permissions", HttpMethod.Put, JsonContent.Create(new
            {
                remove = (~permission.GetHashCode()).ToString(),
                add = permission.GetHashCode().ToString()
            }), sender);
            return respone?.IsSuccessStatusCode ?? false;
        }
    }
}
