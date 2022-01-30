using System.Net.Http.Json;
using QQChannelBot.Models;

namespace QQChannelBot.Bot
{
    public partial class BotClient
    {
        /// <summary>
        /// 音频控制
        /// </summary>
        /// <param name="channel_id">子频道Id</param>
        /// <param name="audioControl">音频对象</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<Message?> AudioControlAsync(string channel_id, AudioControl audioControl, Sender? sender = null)
        {
            BotAPI api = APIList.音频控制;
            HttpResponseMessage? respone = await HttpSendAsync(api.Path.Replace("{channel_id}", channel_id), api.Method, JsonContent.Create(audioControl), sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<Message?>();
        }
    }
}
