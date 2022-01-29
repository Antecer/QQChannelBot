using System.Net.Http.Json;
using QQChannelBot.Models;

namespace QQChannelBot.Bot
{
    public partial class BotClient
    {
        /// <summary>
        /// 获取子频道信息
        /// </summary>
        /// <param name="channel_id">子频道Id</param>
        /// <param name="sender"></param>
        /// <returns>Channel?</returns>
        public async Task<Channel?> GetChannelAsync(string channel_id, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}", null, null, sender);
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
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/guilds/{guild_id}/channels", null, null, sender);
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
        /// <param name="guild_id">频道Id</param>
        /// <param name="name">子频道名称</param>
        /// <param name="type">子频道类型</param>
        /// <param name="subType">子频道子类型</param>
        /// <param name="privateType">子频道私密类型</param>
        /// <param name="position">子频道排序</param>
        /// <param name="parent_id">子频道所属分组Id</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<Channel?> CreateChannelAsync(string guild_id, string name, ChannelType type = ChannelType.文字, ChannelSubType subType = ChannelSubType.闲聊, ChannelPrivateType privateType = ChannelPrivateType.Public, int position = 0, string? parent_id = null, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/guilds/{guild_id}/channels", HttpMethod.Post, JsonContent.Create(new
            {
                name,
                type = type.GetHashCode(),
                sub_type = subType.GetHashCode(),
                private_type = privateType.GetHashCode(),
                position,
                parent_id
            }), sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<Channel?>();
        }
        /// <summary>
        /// 创建子频道（仅私域可用）
        /// </summary>
        /// <param name="channel">用于创建子频道的对象（需提前填充必要字段）</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<Channel?> CreateChannelAsync(Channel channel, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/guilds/{channel.GuildId}/channels", HttpMethod.Post, JsonContent.Create(channel), sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<Channel?>();
        }
        /// <summary>
        /// 修改子频道（仅私域可用）
        /// </summary>
        /// <param name="channel_id">子频道Id</param>
        /// <param name="name">子频道名称</param>
        /// <param name="type">子频道类型</param>
        /// <param name="subType">子频道子类型</param>
        /// <param name="privateType">子频道私密类型</param>
        /// <param name="position">子频道排序</param>
        /// <param name="parent_id">子频道所属分组Id</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<Channel?> EditChannelAsync(string channel_id, string name, ChannelType type, ChannelSubType subType, ChannelPrivateType privateType, int position, string? parent_id, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}", HttpMethod.Patch, JsonContent.Create(new
            {
                name,
                type = type.GetHashCode(),
                sub_type = subType.GetHashCode(),
                private_type = privateType.GetHashCode(),
                position,
                parent_id
            }), sender);
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
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel.Id}", HttpMethod.Patch, JsonContent.Create(channel), sender);
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
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}", HttpMethod.Delete, null, sender);
            return respone?.IsSuccessStatusCode ?? false;
        }
    }
}
