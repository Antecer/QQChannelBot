using System.Net.Http.Json;
using System.Text.Json;
using QQChannelBot.Models;

namespace QQChannelBot.Bot
{
    public partial class BotClient
    {
        /// <summary>
        /// 获取通用 WSS 接入点
        /// </summary>
        /// <returns>一个用于连接 websocket 的地址</returns>
        public async Task<string?> GetWssUrl()
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/gateway");
            JsonElement? res = respone == null ? null : await respone.Content.ReadFromJsonAsync<JsonElement?>();
            return res?.GetProperty("url").GetString();
        }
        /// <summary>
        /// 获取带分片 WSS 接入点
        /// <para>
        /// 详情查阅: <see href="https://bot.q.qq.com/wiki/develop/api/openapi/wss/shard_url_get.html">QQ机器人文档</see>
        /// </para>
        /// </summary>
        /// <returns>一个用于连接 websocket 的地址。<br/>同时返回建议的分片数，以及目前连接数使用情况。</returns>
        public async Task<WebSocketLimit?> GetWssUrlWithShared()
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/gateway/bot");
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<WebSocketLimit?>();
        }
    }
}
