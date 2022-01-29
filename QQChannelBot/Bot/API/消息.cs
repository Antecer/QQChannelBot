using System.Net.Http.Json;
using QQChannelBot.Models;

namespace QQChannelBot.Bot
{
    public partial class BotClient
    {
        /// <summary>
        /// 获取消息列表（2022年1月29日暂未开通）
        /// </summary>
        /// <param name="message">作为坐标的消息（需要消息Id和子频道Id）</param>
        /// <param name="limit">分页大小（1-20）</param>
        /// <param name="typesEnum">拉取类型（默认拉取最新消息）</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<List<Message>?> GetMessagesAsync(Message message, int limit = 20, GetMsgTypesEnum? typesEnum = null, Sender? sender = null)
        {
            string type = typesEnum == null ? "" : $"&type={typesEnum}";
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{message.ChannelId}/messages?limit={limit}&id={message.Id}{type}", null, null, sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<List<Message>?>();
        }
        /// <summary>
        /// 获取指定消息
        /// </summary>
        /// <param name="channel_id">子频道Id</param>
        /// <param name="message_id">消息Id</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<Message?> GetMessageAsync(string channel_id, string message_id, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/messages/{message_id}", null, null, sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<Message?>();
        }
        /// <summary>
        /// 发送消息
        /// <para>
        /// 要求操作人在该子频道具有"发送消息"的权限 <br/>
        /// 发送成功之后，会触发一个创建消息的事件 <br/>
        /// 被动回复消息有效期为 5 分钟 <br/>
        /// 主动推送消息每个子频道限 2 条/天 <br/>
        /// 发送消息接口要求机器人接口需要链接到websocket gateway 上保持在线状态
        /// </para>
        /// </summary>
        /// <param name="channel_id">子频道Id</param>
        /// <param name="message">消息对象</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<Message?> SendMessageAsync(string channel_id, MessageToCreate message, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/messages", HttpMethod.Post, JsonContent.Create(message), sender);
            Message? result = respone == null ? null : await respone.Content.ReadFromJsonAsync<Message?>();
            return LastMessage(result, true);
        }
        /// <summary>
        /// 撤回消息
        /// </summary>
        /// <param name="channel_id">子频道Id</param>
        /// <param name="message_id">消息Id</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<bool> DeleteMessageAsync(string channel_id, string message_id, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/messages/{message_id}", HttpMethod.Delete, null, sender);
            return respone?.IsSuccessStatusCode ?? false;
        }
        /// <summary>
        /// 撤回目标用户在当前子频道发出的最后一条消息
        /// <para>
        /// 需要传入指令发出者的消息对象<br/>
        /// 用于检索指令发出者所在频道信息
        /// </para>
        /// </summary>
        /// <param name="masterMessage">
        /// 被撤回消息的目标用户信息<br/>
        /// 需要：message.GuildId、message.ChannelId、message.Author.Id
        /// </param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<bool?> DeleteLastMessageAsync(Message? masterMessage, Sender? sender = null)
        {
            Message? lastMessage = LastMessage(masterMessage);
            return lastMessage == null ? null : await DeleteMessageAsync(lastMessage.ChannelId, lastMessage.Id, sender);
        }
    }
}
