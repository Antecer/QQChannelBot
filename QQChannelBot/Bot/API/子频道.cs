using System.Net.Http.Json;
using QQChannelBot.Models;

namespace QQChannelBot.Bot
{
    public partial class BotClient
    {
        /// <summary>
        /// 获取子频道详情
        /// </summary>
        /// <param name="channel_id">子频道Id</param>
        /// <param name="sender"></param>
        /// <returns>Channel?</returns>
        public async Task<Channel?> GetChannelAsync(string channel_id, Sender? sender = null)
        {
            BotAPI api = APIList.获取子频道详情;
            HttpResponseMessage? respone = await HttpSendAsync(api.Path.Replace("{channel_id}", channel_id), api.Method, null, sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<Channel?>();
        }
        /// <summary>
        /// 获取频道下的子频道列表
        /// </summary>
        /// <param name="guild_id">频道Id</param>
        /// <param name="channelType">筛选子频道类型</param>
        /// <param name="channelSubType">筛选子频道子类型</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<List<Channel>?> GetChannelsAsync(string guild_id, ChannelType? channelType = null, ChannelSubType? channelSubType = null, Sender? sender = null)
        {
            BotAPI api = APIList.获取子频道列表;
            HttpResponseMessage? respone = await HttpSendAsync(api.Path.Replace("{guild_id}", guild_id), api.Method, null, sender);
            List<Channel>? channels = respone == null ? null : await respone.Content.ReadFromJsonAsync<List<Channel>?>();
            if (channels != null)
            {
                if (channelType != null) channels = channels.Where(channel => channel.Type == channelType).ToList();
                if (channelSubType != null) channels = channels.Where(channel => channel.SubType == channelSubType).ToList();
            }
            return channels;
        }
        /// <summary>
        /// 创建子频道（仅私域可用）
        /// </summary>
        /// <param name="channel">用于创建子频道的对象（需提前填充必要字段）</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<Channel?> CreateChannelAsync(Channel channel, Sender? sender = null)
        {
            BotAPI api = APIList.创建子频道;
            HttpResponseMessage? respone = await HttpSendAsync(api.Path.Replace("{guild_id}", channel.GuildId), api.Method, JsonContent.Create(channel), sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<Channel?>();
        }
        /// <summary>
        /// 修改子频道（仅私域可用）
        /// </summary>
        /// <param name="channel">修改属性后的子频道对象</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<Channel?> EditChannelAsync(Channel channel, Sender? sender = null)
        {
            BotAPI api = APIList.修改子频道;
            HttpResponseMessage? respone = await HttpSendAsync(api.Path.Replace("{channel_id}", channel.Id), api.Method, JsonContent.Create(channel), sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<Channel?>();
        }
        /// <summary>
        /// 删除指定子频道（仅私域可用）
        /// </summary>
        /// <param name="channel_id">要删除的子频道Id</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<bool> DeleteChannelAsync(string channel_id, Sender? sender = null)
        {
            BotAPI api = APIList.删除子频道;
            HttpResponseMessage? respone = await HttpSendAsync(api.Path.Replace("{channel_id}", channel_id), api.Method, null, sender);
            return respone?.IsSuccessStatusCode ?? false;
        }
    }
}
