using System.Net.Http.Json;
using QQChannelBot.Models;

namespace QQChannelBot.Bot
{
    public partial class BotClient
    {
        /// <summary>
        /// 获取频道可用权限列表
        /// <para>
        /// 获取机器人在频道 guild_id 内可以使用的权限列表
        /// </para>
        /// </summary>
        /// <param name="guild_id">频道Id</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<List<APIPermission>?> GetGuildPermissionsAsync(string guild_id, Sender? sender = null)
        {
            BotAPI api = APIList.获取频道可用权限列表;
            HttpResponseMessage? respone = await HttpSendAsync(api.Path.Replace("{guild_id}", guild_id), api.Method, null, sender);
            APIPermissions? Permissions = respone == null ? null : await respone.Content.ReadFromJsonAsync<APIPermissions?>();
            return Permissions?.List;
        }
        /// <summary>
        /// 创建频道 API 接口权限授权链接
        /// </summary>
        /// <param name="guild_id">频道Id</param>
        /// <param name="channel_id">子频道Id</param>
        /// <param name="api_identify">权限需求标识对象</param>
        /// <param name="desc">机器人申请对应的 API 接口权限后可以使用功能的描述</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<APIPermissionDemand?> SendPermissionDemandAsync(string guild_id, string channel_id, APIPermissionDemandIdentify api_identify, string desc = "", Sender? sender = null)
        {
            BotAPI api = APIList.创建频道接口授权链接;
            HttpResponseMessage? respone = await HttpSendAsync(api.Path.Replace("{guild_id}", guild_id), api.Method, JsonContent.Create(new
            {
                channel_id,
                api_identify,
                desc
            }), sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<APIPermissionDemand?>();
        }
    }
}
