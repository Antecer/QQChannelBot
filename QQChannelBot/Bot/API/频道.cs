using System.Net.Http.Json;
using QQChannelBot.Models;

namespace QQChannelBot.Bot
{
    public partial class BotClient
    {
        /// <summary>
        /// 获取频道详情
        /// </summary>
        /// <param name="guild_id">频道Id</param>
        /// <param name="sender"></param>
        /// <returns>Guild?</returns>
        public async Task<Guild?> GetGuildAsync(string guild_id, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/guilds/{guild_id}", null, null, sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<Guild?>();
        }
    }
}
