using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using QQChannelBot.BotApi.SocketEvent;
using QQChannelBot.Models;
using QQChannelBot.MsgHelper;

namespace QQChannelBot.BotApi
{
    public class ChannelBot
    {
        #region 可以监听的固定事件列表
        /// <summary>
        /// WebSocketClient连接后触发
        /// </summary>
        public event Action<ChannelBot>? OnWebSocketConnected;
        /// <summary>
        /// WebSocketClient关闭后触发
        /// </summary>
        public event Action<ChannelBot>? OnWebSocketClosed;
        /// <summary>
        /// WebSocketClient发送数据前触发
        /// </summary>
        public event Action<ChannelBot, string>? OnWebSoketSending;
        /// <summary>
        /// WebSocketClient收到数据后触发
        /// </summary>
        public event Action<ChannelBot, string>? OnWebSocketReceived;
        /// <summary>
        /// 收到服务端推送的消息时触发
        /// </summary>
        public event Action<ChannelBot, JsonElement>? OnDispatch;
        /// <summary>
        /// 客户端发送心跳或收到服务端推送心跳时触发
        /// </summary>
        public event Action<ChannelBot, JsonElement>? OnHeartbeat;
        /// <summary>
        /// 客户端发送鉴权时触发
        /// </summary>
        public event Action<ChannelBot, JsonElement>? OnIdentify;
        /// <summary>
        /// 客户端恢复连接时触发
        /// </summary>
        public event Action<ChannelBot, JsonElement>? OnResume;
        /// <summary>
        /// 服务端通知客户端重新连接时触发
        /// </summary>
        public event Action<ChannelBot, JsonElement>? OnReconnect;
        /// <summary>
        /// 当identify或resume的时候，参数错误的时候触发
        /// </summary>
        public event Action<ChannelBot, JsonElement>? OnInvalidSession;
        /// <summary>
        /// 当客户端与网关建立ws连接的时候触发
        /// </summary>
        public event Action<ChannelBot, JsonElement>? OnHello;
        /// <summary>
        /// 客户端发送心跳被服务端接收后触发
        /// </summary>
        public event Action<ChannelBot, JsonElement>? OnHeartbeatACK;
        /// <summary>
        /// 鉴权连接成功后触发
        /// <para>注:此时获取的User对象只有3个属性 {id,username,bot}</para>
        /// </summary>
        public event Action<ChannelBot, User?>? OnReady;
        /// <summary>
        /// 恢复连接成功后触发
        /// </summary>
        public event Action<ChannelBot, JsonElement?>? OnResumed;
        /// <summary>
        /// 频道信息变更后触发
        /// <para>加入频道, 资料变更, 退出频道</para>
        /// </summary>
        public event Action<ChannelBot, JsonElement?, ActionType>? OnGuildMsg;
        /// <summary>
        /// 子频道被修改后触发
        /// <para>创建子频道, 更新子频道, 删除子频道</para>
        /// </summary>
        public event Action<ChannelBot, JsonElement?, ActionType>? OnChannelMsg;
        /// <summary>
        /// 成员信息变更后触发
        /// <para>成员加入, 资料变更, 移除成员</para>
        /// </summary>
        public event Action<ChannelBot, JsonElement?, ActionType>? OnGuildMemberMsg;
        /// <summary>
        /// 修改表情表态后触发
        /// <para>添加表情表态, 删除表情表态</para>
        /// </summary>
        public event Action<ChannelBot, JsonElement?, ActionType>? OnMessageReaction;
        /// <summary>
        /// 机器人收到私信后触发
        /// </summary>
        public event Action<ChannelBot, JsonElement?, ActionType>? OnDirectMessage;
        /// <summary>
        /// 音频状态变更后触发
        /// </summary>
        public event Action<ChannelBot, JsonElement?, ActionType>? OnAudioMsg;
        /// <summary>
        /// 收到 @机器人 消息后触发
        /// </summary>
        public event Action<ChannelBot, Message, ActionType>? OnAtMessage;
        #endregion

        /// <summary>
        /// 鉴权信息
        /// <para>可在这里查询 <see href="https://bot.q.qq.com/#/developer/developer-setting">QQ机器人开发设置</see></para>
        /// </summary>
        public Identity BotAccessInfo { get; set; }
        /// <summary>
        /// 机器人用户信息
        /// </summary>
        public User? UserInfo { get; set; }

        #region Http客户端配置
        /// <summary>
        /// Http客户端
        /// </summary>
        private static HttpClient WebHttpClient { get; set; } = new(new HttpLoggingHandler(new HttpClientHandler()));
        /// <summary>
        /// 正式环境
        /// </summary>
        const string releaseApi = "https://api.sgroup.qq.com";
        /// <summary>
        /// 沙箱环境
        /// <para>
        /// 沙箱环境只会收到测试频道的事件，且调用openapi仅能操作测试频道
        /// </para>
        /// </summary>
        const string sandboxApi = "https://sandbox.api.sgroup.qq.com";
        /// <summary>
        /// 启用沙箱模式
        /// </summary>
        public bool SandBox { get; set; } = false;
        /// <summary>
        /// 机器人接口域名
        /// </summary>
        public string ApiOrigin { get => SandBox ? sandboxApi : releaseApi; }
        #endregion

        #region Socket客户端配置
        /// <summary>
        /// Socket客户端
        /// </summary>
        private static ClientWebSocket WebSocketClient { get; set; } = new();
        /// <summary>
        /// Socket客户端收到的最新的消息的s，如果是第一次连接，传null
        /// </summary>
        private int? WssMsgLastS { get; set; } = null;
        /// <summary>
        /// 此次连接所需要接收的事件
        /// <para>具体可参考 <see href="https://bot.q.qq.com/wiki/develop/api/gateway/intents.html">Intents</see></para>
        /// </summary>
        public Intent Intents { get; set; } = Intent.GUILDS | Intent.GUILD_MEMBERS | Intent.AT_MESSAGES;
        /// <summary>
        /// 会话限制
        /// </summary>
        private WebSocketLimit GateLimit { get; set; } = new();
        /// <summary>
        /// 分片id
        /// <para>
        /// 分片是按照频道id进行哈希的，同一个频道的信息会固定从同一个链接推送。<br/>
        /// 详见 <see href="https://bot.q.qq.com/wiki/develop/api/gateway/shard.html">Shard机制</see>
        /// </para>
        /// </summary>
        private int ShardId { get; set; } = 0;
        /// <summary>
        /// Socket客户端存储的SessionId
        /// </summary>
        private string? WebSoketSessionId { get; set; }
        /// <summary>
        /// WebSocket接收缓冲区
        /// </summary>
        private ArraySegment<byte> ReceiveBuffer { get; set; } = new(new byte[8192]);
        /// <summary>
        /// 心跳周期(单位毫秒)
        /// </summary>
        private int HeartbeatInterval { get; set; } = 0;
        #endregion

        /// <summary>
        /// QQ频道机器人
        /// </summary>
        /// <param name="debugMode">启用调试模式</param>
        public ChannelBot(Identity identity, bool? sandBox = null)
        {
            BotAccessInfo = identity;
            if (sandBox != null) SandBox = sandBox.Value;
        }

        #region 自定义指令注册
        /// <summary>
        /// 缓存动态注册的消息指令事件
        /// </summary>
        private static readonly Dictionary<string, Action<ChannelBot, Message, string>> Commands = new();
        /// <summary>
        /// 注册消息指令
        /// <para>注: 被指令命中的消息不会触发 AtMessageAction 事件</para>
        /// </summary>
        /// <param name="command">指令名称</param>
        /// <param name="commandAction">回调函数</param>
        /// <param name="displace">指令名称重复的处理办法<para>true:替换, false:忽略</para></param>
        /// <returns></returns>
        public ChannelBot AddCommand(string command, Action<ChannelBot, Message, string> commandAction, bool displace = false)
        {
            if (Commands.ContainsKey(command))
            {
                if (displace)
                {
                    Log.Warn($"[RegisterCommand] 指令 {command} 已存在,已替换新注册的功能!");
                    Commands[command] = commandAction;
                }
                else Log.Warn($"[RegisterCommand] 指令 {command} 已存在,已忽略新功能的注册!");
            }
            else
            {
                Log.Info($"[RegisterCommand] 指令 {command} 已注册.");
                Commands[command] = commandAction;
            }
            return this;
        }
        /// <summary>
        /// 删除消息指令
        /// </summary>
        /// <param name="command">指令名称</param>
        /// <returns></returns>
        public ChannelBot DelCommand(string command)
        {
            if (Commands.ContainsKey(command))
            {
                Log.Info($"[RegisterCommand] 指令 {command} 已删除.");
                Commands.Remove(command);
            }
            else Log.Warn($"[RegisterCommand] 指令 {command} 不存在!");
            return this;
        }
        #endregion

        #region 频道API
        /// <summary>
        /// 获取频道详情
        /// </summary>
        /// <param name="guild_Id">频道id</param>
        /// <returns>Guild?</returns>
        public async Task<Guild?> GetGuildAsync(string guild_id)
        {
            return await WebHttpClient.GetFromJsonAsync<Guild>($"{ApiOrigin}/guilds/{guild_id}");
        }
        #endregion

        #region 频道身份组API
        /// <summary>
        /// 获取频道身份组列表
        /// </summary>
        /// <param name="guild_id">频道id</param>
        /// <returns></returns>
        public async Task<GuildRoles?> GetGuildRolesAsync(string guild_id)
        {
            return await WebHttpClient.GetFromJsonAsync<GuildRoles>($"{ApiOrigin}/guilds/{guild_id}/roles");
        }
        /// <summary>
        /// 创建频道身份组
        /// </summary>
        /// <param name="guild_id">频道id</param>
        /// <param name="filter">标识需要设置哪些字段</param>
        /// <param name="info">携带需要设置的字段内容</param>
        /// <returns></returns>
        public async Task<CreateRoleRes?> CreateRoleAsync(string guild_id, Filter filter, Info info)
        {
            HttpResponseMessage res = await WebHttpClient.PostAsJsonAsync($"{ApiOrigin}/guilds/{guild_id}/roles", new { filter, info });
            return await res.Content.ReadFromJsonAsync<CreateRoleRes>();
        }
        /// <summary>
        /// 修改频道身份组
        /// </summary>
        /// <param name="guild_id"></param>
        /// <param name="role_id"></param>
        /// <param name="filter">标识需要修改哪些字段</param>
        /// <param name="info">携带需要修改的字段内容</param>
        /// <returns></returns>
        public async Task<ModifyRolesRes?> ModifyRolesAsync(string guild_id, string role_id, Filter filter, Info info)
        {
            HttpResponseMessage res = await WebHttpClient.PatchAsync($"{ApiOrigin}/guilds/{guild_id}/roles/{role_id}", JsonContent.Create(new { filter, info }));
            return await res.Content.ReadFromJsonAsync<ModifyRolesRes>();
        }
        /// <summary>
        /// 删除身份组
        /// </summary>
        /// <param name="guild_id">频道id</param>
        /// <param name="role_id">身份Id</param>
        /// <returns></returns>
        public async Task DeleteRoleAsync(string guild_id, string role_id)
        {
            await WebHttpClient.DeleteAsync($"{ApiOrigin}/guilds/{guild_id}/roles/{role_id}");
        }
        /// <summary>
        /// 增加频道身份组成员
        /// <para>
        /// 需要使用的 token 对应的用户具备增加身份组成员权限。如果是机器人，要求被添加为管理员。 <br/>
        /// 如果要增加的身份组ID是(5-子频道管理员)，需要增加 channel_id 来指定具体是哪个子频道。
        /// </para>
        /// </summary>
        /// <param name="guild_id">频道id</param>
        /// <param name="user_id">用户id</param>
        /// <param name="role_id">身份组id</param>
        /// <param name="channel_id">子频道id</param>
        /// <returns></returns>
        public async Task AddMemberToRoleAsync(string guild_id, string user_id, string role_id, string? channel_id = null)
        {
            if (channel_id == null)
            {
                await WebHttpClient.PutAsync($"{ApiOrigin}/guilds/{guild_id}/members/{user_id}/roles/{role_id}", null);
            }
            else
            {
                await WebHttpClient.PutAsJsonAsync($"{ApiOrigin}/guilds/{guild_id}/members/{user_id}/roles/{role_id}", new Channel { Id = channel_id });
            }
        }
        /// <summary>
        /// 删除频道身份组成员
        /// <para>
        /// 需要使用的 token 对应的用户具备删除身份组成员权限。如果是机器人，要求被添加为管理员。 <br/>
        /// 如果要删除的身份组ID是(5-子频道管理员)，需要增加 channel_id 来指定具体是哪个子频道。 <br/>
        /// 详情查阅 <see href="https://bot.q.qq.com/wiki/develop/api/openapi/guild/delete_guild_member_role.html">QQ机器人文档</see>
        /// </para>
        /// </summary>
        /// <param name="guild_id">频道id</param>
        /// <param name="user_id">用户id</param>
        /// <param name="role_id">身份组id</param>
        /// <param name="channel_id">子频道id</param>
        /// <returns></returns>
        public async Task DeleteMemberToRoleAsync(string guild_id, string user_id, string role_id, string? channel_id = null)
        {
            if (channel_id == null)
            {
                await WebHttpClient.DeleteAsync($"{ApiOrigin}/guilds/{guild_id}/members/{user_id}/roles/{role_id}");
            }
            else
            {
                Console.WriteLine("[删除频道身份组成员] 删除子频道管理员身份组功能暂未实现");
            }
        }
        #endregion

        #region 成员API
        /// <summary>
        /// 获取频道成员详情
        /// </summary>
        /// <param name="guild_id">频道id</param>
        /// <param name="user_id">用户id</param>
        /// <returns></returns>
        public async Task<Member?> GetGuildMemberAsync(string guild_id, string user_id)
        {
            return await WebHttpClient.GetFromJsonAsync<Member>($"{ApiOrigin}/guilds/{guild_id}/members/{user_id}");
        }
        #endregion

        #region 公告API
        /// <summary>
        /// 创建子频道公告
        /// <para>
        /// 机器人设置消息为指定子频道公告
        /// </para>
        /// </summary>
        /// <param name="channel_id">子频道id</param>
        /// <param name="message_id">消息id</param>
        /// <returns></returns>
        public async Task<Announces?> CreateAnnouncesAsync(string channel_id, string message_id)
        {
            HttpResponseMessage res = await WebHttpClient.PostAsJsonAsync($"{ApiOrigin}/channels/{channel_id}/announces", new { message_id });
            return await res.Content.ReadFromJsonAsync<Announces>();
        }
        /// <summary>
        /// 删除子频道公告
        /// <para>
        /// 机器人删除指定子频道公告
        /// </para>
        /// </summary>
        /// <param name="channel_id">子频道id</param>
        /// <param name="message_id">消息id</param>
        /// <returns></returns>
        public async Task DeleteAnnouncesAsync(string channel_id, string message_id)
        {
            await WebHttpClient.DeleteAsync($"{ApiOrigin}/channels/{channel_id}/announces/{message_id}");
        }
        #endregion

        #region 子频道API
        /// <summary>
        /// 获取子频道信息
        /// </summary>
        /// <param name="channel_id">子频道id</param>
        /// <returns>Channel?</returns>
        public async Task<Channel?> GetChannelAsync(string channel_id)
        {
            return await WebHttpClient.GetFromJsonAsync<Channel>($"{ApiOrigin}/channels/{channel_id}");
        }
        /// <summary>
        /// 获取频道下的子频道列表
        /// </summary>
        /// <param name="guild_id">频道id</param>
        /// <param name="channelType">筛选子频道类型</param>
        /// <param name="channelSubType">筛选子频道子类型</param>
        /// <returns></returns>
        public async Task<List<Channel>> GetChannelsAsync(string guild_id, ChannelType? channelType = null, ChannelSubType? channelSubType = null)
        {
            List<Channel> channelList = new() { };
            List<Channel>? channels = await WebHttpClient.GetFromJsonAsync<List<Channel>>($"{ApiOrigin}/guilds/{guild_id}/channels");
            if (channels != null) channelList.AddRange(channels);
            if (channelType != null)
            {
                channelList = channelList.Where(channel => channel.Type == channelType).ToList();
                if (channelSubType != null) channelList = channelList.Where(channel => channel.SubType == channelSubType).ToList();
            }
            return channelList;
        }
        #endregion

        #region 子频道权限API
        /// <summary>
        /// 获取指定子频道的权限
        /// </summary>
        /// <param name="channel_id">子频道id</param>
        /// <param name="user_id">用户id</param>
        /// <returns></returns>
        public async Task<ChannelPermissions?> GetChannelPermissionsAsync(string channel_id, string user_id)
        {
            return await WebHttpClient.GetFromJsonAsync<ChannelPermissions>($"{ApiOrigin}/channels/{channel_id}/members/{user_id}/permissions");
        }
        /// <summary>
        /// 修改指定子频道的权限
        /// </summary>
        /// <param name="channel_id">子频道id</param>
        /// <param name="user_id"></param>
        /// <param name="add"></param>
        /// <param name="remove"></param>
        /// <returns></returns>
        public async Task<ChannelPermissions?> ModifyChannelPermissionsAsync(string channel_id, string user_id, string add = "0", string remove = "0")
        {
            HttpResponseMessage res = await WebHttpClient.PutAsJsonAsync<object>($"{ApiOrigin}/channels/{channel_id}/members/{user_id}/permissions", new { add, remove });
            return await res.Content.ReadFromJsonAsync<ChannelPermissions>();
        }
        #endregion

        #region 消息API
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
        /// <param name="channel_id">子频道 id</param>
        /// <param name="message">消息对象</param>
        /// <returns></returns>
        public async Task<Message?> SendMessageAsync(string channel_id, MessageToCreate message)
        {
            HttpResponseMessage res = await WebHttpClient.PostAsJsonAsync($"{ApiOrigin}/channels/{channel_id}/messages", message);
            if (!res.IsSuccessStatusCode) Log.Debug(await res.Content.ReadAsStringAsync());
            return await res.Content.ReadFromJsonAsync<Message>();
        }
        /// <summary>
        /// 发送文本消息 (可使用内嵌格式)
        /// <para>
        /// &lt;@user_id&gt; 解析为 @用户 标签 <br/>
        /// &lt;#channel_id&gt; 解析为 #子频道 标签，点击可以跳转至子频道，仅支持当前频道内的子频道
        /// </para>
        /// </summary>
        /// <param name="channel_id">子频道id</param>
        /// <param name="content">消息内容字符串</param>
        /// <param name="msg_id">要回复的消息id</param>
        /// <returns></returns>
        public async Task<Message?> SendMessageAsync(string channel_id, string content, string msg_id = "")
        {
            return await SendMessageAsync(channel_id, new MessageToCreate { Content = content, MsgId = msg_id });
        }
        /// <summary>
        /// 发送图片消息
        /// </summary>
        /// <param name="channel_id">子频道id</param>
        /// <param name="image">图片url</param>
        /// <param name="msg_id">要回复的消息id</param>
        /// <returns></returns>
        public async Task<Message?> SendImageAsync(string channel_id, string image, string msg_id = "")
        {
            return await SendMessageAsync(channel_id, new MessageToCreate { Image = image, MsgId = msg_id });
        }
        #endregion

        #region 音频API
        /// <summary>
        /// 音频控制
        /// </summary>
        /// <param name="channel_id">子频道 id</param>
        /// <param name="audio_url">音频数据的 url, status 为 0 时传</param>
        /// <param name="text">状态文本（比如：简单爱-周杰伦），可选; status 为 0 时传，其他操作不传</param>
        /// <param name="status">播放状态，参考 STATUS</param>
        /// <returns></returns>
        public async Task<Message?> AudioControlAsync(string channel_id, string audio_url, string text = "", STATUS status = STATUS.START)
        {

            HttpResponseMessage res = await WebHttpClient.PostAsJsonAsync($"{ApiOrigin}/channels/{channel_id}/audio", new AudioControl { AudioUrl = audio_url, Text = text, Status = status });
            return await res.Content.ReadFromJsonAsync<Message>();
        }
        #endregion

        #region 用户API
        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        /// <returns>当前用户对象</returns>
        public async Task<User?> GetMeAsync()
        {
            return await WebHttpClient.GetFromJsonAsync<User>($"{ApiOrigin}/users/@me");
        }
        /// <summary>
        /// 获取机器人所在频道列表 // 还有其他参数
        /// </summary>
        /// <returns>频道列表</returns>
        public async Task<List<Guild>?> GetMeGuildsAsync()
        {
            return await WebHttpClient.GetFromJsonAsync<List<Guild>>($"{ApiOrigin}/users/@me/guilds");
        }
        #endregion

        #region 日程API
        /// <summary>
        /// 获取频道日程列表
        /// <para>
        /// 获取某个日程子频道里中当天的日程列表; <br/>
        /// 若带了参数 since，则返回结束时间在 since 之后的日程列表; <br/>
        /// 若未带参数 since，则默认返回当天的日程列表。
        /// </para>
        /// </summary>
        /// <param name="channel_id">日程子频道id</param>
        /// <param name="since">起始时间戳(ms)</param>
        /// <returns>List&lt;Schedule&gt;?</returns>
        public async Task<List<Schedule>?> GetSchedulesAsync(string channel_id, ulong? since = null)
        {
            string param = since == null ? "" : $"?since={since}";
            return await WebHttpClient.GetFromJsonAsync<List<Schedule>>($"{ApiOrigin}/channels/{channel_id}/schedules{param}");
        }
        /// <summary>
        /// 获取单个日程信息
        /// </summary>
        /// <param name="channel_id">日程子频道id</param>
        /// <param name="schedule_id">日程id</param>
        /// <returns>目标 Schedule 对象</returns>
        public async Task<Schedule?> GetScheduleAsync(string channel_id, string schedule_id)
        {
            return await WebHttpClient.GetFromJsonAsync<Schedule>($"{ApiOrigin}/channels/{channel_id}/schedules/{schedule_id}");
        }
        /// <summary>
        /// 创建日程
        /// <para>
        /// 要求操作人具有"管理频道"的权限，如果是机器人，则需要将机器人设置为管理员。<br/>
        /// 创建成功后，返回创建成功的日程对象。
        /// </para>
        /// </summary>
        /// <param name="channel_id">日程子频道id</param>
        /// <param name="schedule">日程对象，不需要带id</param>
        /// <returns>新创建的 Schedule 对象</returns>
        public async Task<Schedule?> CreateScheduleAsync(string channel_id, Schedule schedule)
        {
            HttpResponseMessage res = await WebHttpClient.PostAsJsonAsync($"{ApiOrigin}/channels/{channel_id}/schedules", new { schedule });
            return await res.Content.ReadFromJsonAsync<Schedule>();
        }
        /// <summary>
        /// 修改日程
        /// <para>
        /// 要求操作人具有"管理频道"的权限，如果是机器人，则需要将机器人设置为管理员。<br/>
        /// 修改成功后，返回修改后的日程对象。
        /// </para>
        /// </summary>
        /// <param name="channel_id">日程子频道id</param>
        /// <param name="schedule">日程对象</param>
        /// <returns>修改后的 Schedule 对象</returns>
        public async Task<Schedule?> ModifyScheduleAsync(string channel_id, Schedule schedule)
        {
            HttpResponseMessage res = await WebHttpClient.PatchAsync($"{ApiOrigin}/channels/{channel_id}/schedules/{schedule.Id}", JsonContent.Create(schedule));
            return await res.Content.ReadFromJsonAsync<Schedule>();
        }
        /// <summary>
        /// 删除日程
        /// <para>
        /// 要求操作人具有"管理频道"的权限，如果是机器人，则需要将机器人设置为管理员。
        /// </para>
        /// </summary>
        /// <param name="channel_id">日程子频道id</param>
        /// <param name="schedule_id">日程id</param>
        /// <returns>HTTP 状态码 204</returns>
        public async Task<string> DeleteScheduleAsync(string channel_id, string schedule_id)
        {
            HttpResponseMessage res = await WebHttpClient.DeleteAsync($"{ApiOrigin}/channels/{channel_id}/schedules/{schedule_id}");
            return await res.Content.ReadAsStringAsync();
        }
        #endregion

        #region WebSocketAPI 
        /// <summary>
        /// 获取通用 WSS 接入点
        /// </summary>
        /// <returns>一个用于连接 websocket 的地址</returns>
        public async Task<string?> GetWssUrl()
        {
            JsonElement res = await WebHttpClient.GetFromJsonAsync<JsonElement>($"{ApiOrigin}/gateway");
            return res.GetProperty("url").GetString();
        }
        /// <summary>
        /// 获取带分片 WSS 接入点
        /// <para>
        /// 详情查阅: <see href="https://bot.q.qq.com/wiki/develop/api/openapi/wss/shard_url_get.html">QQ机器人文档</see>
        /// </para>
        /// </summary>
        /// <returns>一个用于连接 websocket 的地址。<br/>同时返回建议的分片数，以及目前连接数使用情况。</returns>
        public async Task<string?> GetWssUrlWithShared()
        {
            GateLimit = await WebHttpClient.GetFromJsonAsync<WebSocketLimit>($"{ApiOrigin}/gateway/bot") ?? new();
            return GateLimit?.Url;
        }
        #endregion

        #region WebSocket
        /// <summary>
        /// 关闭WebSocket连接并立即释放占用的资源
        /// </summary>
        /// <param name="desc">关闭原因</param>
        /// <returns></returns>
        public void CloseWebSocket(string desc)
        {
            try
            {
                WebSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure, desc, CancellationToken.None);
                Log.Info($"[WebScoket] WebSocketClient Closed!");
                OnWebSocketClosed?.Invoke(this);
            }
            catch (Exception e)
            {
                Log.Warn($"[WebScoket] Close WebSocketClient Error: {e.Message}");
            }
        }
        /// <summary>
        /// 建立到服务器的连接
        /// <para><em>RetryCount</em> 连接失败后允许的重试次数</para>
        /// </summary>
        /// <param name="RetryCount">连接失败后允许的重试次数</param>
        /// <returns></returns>
        public async Task ConnectAsync(int RetryCount)
        {
            WebHttpClient.DefaultRequestHeaders.Authorization = new("Bot", $"{BotAccessInfo.BotAppId}.{BotAccessInfo.BotToken}");
            while (RetryCount-- > 0)
            {
                try
                {
                    string? GatewayUrl = GetWssUrlWithShared().Result;
                    if (Uri.TryCreate(GatewayUrl, UriKind.Absolute, out Uri? webSocketUri))
                    {
                        await WebSocketClient.ConnectAsync(webSocketUri, CancellationToken.None).ConfigureAwait(false);
                        OnWebSocketConnected?.Invoke(this);
                        _ = ReceiveAsync();
                        break;
                    }
                    else
                    {
                        throw new Exception($"Use WssUrl<{GatewayUrl}> Create WebSocketUri Failed!");
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"[WebSocket] Connect Error: {e.Message}");
                    if (RetryCount > 0)
                    {
                        Log.Info($"[WebSocket] Try again in 10 seconds...");
                        CloseWebSocket("ConnectError");
                        await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                    }
                }
            }
        }
        /// <summary>
        /// 鉴权连接
        /// </summary>
        /// <returns></returns>
        private async Task SendIdentifyAsync()
        {
            try
            {
                if (WebSocketClient.State == WebSocketState.Open)
                {
                    var data = new
                    {
                        op = Opcode.Identify,
                        d = new
                        {
                            token = $"Bot {BotAccessInfo.BotAppId}.{BotAccessInfo.BotToken}",
                            intents = Intents,
                            shared = new[] { ShardId % GateLimit.Shards, GateLimit.Shards },
                            properties = new { }
                        }
                    };
                    Log.Info($"[WebSocket] Identify Sending...");
                    await WebSocketSendAsync(JsonSerializer.Serialize(data), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else throw new Exception("WebSocket Connection Broken!");
            }
            catch (Exception e)
            {
                Log.Error($"[WebSocket] Identify Error: {e.Message}");
            }
        }
        /// <summary>
        /// 发送心跳
        /// </summary>
        /// <returns></returns>
        private async Task SendHeartBeatAsync()
        {
            try
            {
                var data = new { op = Opcode.Heartbeat, s = WssMsgLastS };
                Log.Info($"[WebSocket] Heartbeat Sending...");
                await WebSocketSendAsync(JsonSerializer.Serialize(data), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log.Error($"[WebSocket] Heartbeat Error: {e.Message}");
            }
        }
        /// <summary>
        /// 恢复连接
        /// </summary>
        /// <returns></returns>
        private async Task SendResumeAsync()
        {
            try
            {
                var data = new
                {
                    op = Opcode.Resume,
                    d = new
                    {
                        token = $"Bot {BotAccessInfo.BotAppId}.{BotAccessInfo.BotToken}",
                        session_id = WebSoketSessionId,
                        seq = WssMsgLastS
                    }
                };
                Log.Info($"[WebSocket] Resume Sending...");
                await WebSocketSendAsync(JsonSerializer.Serialize(data), WebSocketMessageType.Text, true, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log.Error($"[WebSocket] Resume Error: {e.Message}");
            }
        }
        /// <summary>
        /// WebScoket发送数据到服务端
        /// </summary>
        /// <param name="data">要发送的数据</param>
        /// <param name="msgType">WebSocket消息类型</param>
        /// <param name="endOfMsg">表示数据已发送结束</param>
        /// <param name="cancelToken">用于传播应取消此操作的通知的取消令牌。</param>
        /// <returns></returns>
        private async Task WebSocketSendAsync(string data, WebSocketMessageType msgType = WebSocketMessageType.Text, bool endOfMsg = true, CancellationToken? cancelToken = null)
        {
            Log.Debug($"[WebSocket][Send] {data}");
            OnWebSoketSending?.Invoke(this, data);
            await WebSocketClient.SendAsync(Encoding.UTF8.GetBytes(data), msgType, endOfMsg, cancelToken ?? CancellationToken.None);
        }
        /// <summary>
        /// WebSocket接收服务端数据
        /// </summary>
        /// <returns></returns>
        private async Task ReceiveAsync()
        {
            if (WebSocketClient == null)
            {
                Log.Error($"[WebSocket] Receive Stop: WebSocketClient Is Closed!");
                return;
            }
            try
            {
                WebSocketReceiveResult result = await WebSocketClient.ReceiveAsync(ReceiveBuffer, CancellationToken.None).ConfigureAwait(false);
                if (result == null)
                {
                    Log.Warn($"[WebSocket] Received Warn: No Data Received!");
                }
                else if (result?.MessageType == WebSocketMessageType.Close)
                {
                    Log.Warn($"[WebSocket] Receive: The server sends a Close message.");
                    CloseWebSocket("ServerDisconnected");
                    return;
                }
                else if (result?.MessageType != WebSocketMessageType.Text)
                {
                    Log.Info($"[WebSocket] Received MessageType: {result?.MessageType}");
                }
                else
                {
                    byte[] recBytes = ReceiveBuffer.Skip(ReceiveBuffer.Offset).Take(result.Count).ToArray();
                    string recString = Regex.Unescape(Encoding.UTF8.GetString(recBytes));

                    OnWebSocketReceived?.Invoke(this, recString);
                    await ExcuteCommand(JsonDocument.Parse(recString).RootElement).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Log.Error($"[WebSocket] Receive Error: {e.Message}");
                CloseWebSocket("ReceiveError");
                return;
            }
            await Task.Factory.StartNew(async () => { await ReceiveAsync(); }, TaskCreationOptions.LongRunning).ConfigureAwait(false);
        }

        /// <summary>
        /// 根据收到的数据分析用途
        /// </summary>
        /// <param name="wssJson">Wss接收的数据</param>
        /// <returns></returns>
        private async Task ExcuteCommand(JsonElement wssJson)
        {
            int opcode = wssJson.GetProperty("op").GetInt32();
            switch (opcode)
            {
                // Receive 服务端进行消息推送
                case (int)Opcode.Dispatch:
                    Log.Debug($"[WebSocket][Op00] Dispatch: {wssJson.GetRawText()}");
                    OnDispatch?.Invoke(this, wssJson);
                    WssMsgLastS = wssJson.GetProperty("s").GetInt32();
                    if (!wssJson.TryGetProperty("t", out JsonElement t)) break;
                    string? type = t.GetString();
                    switch (type)
                    {
                        case "READY":
                            await ExcuteCommand(JsonDocument.Parse($"{{\"op\": {(int)Opcode.Heartbeat}, \"d\": null}}").RootElement).ConfigureAwait(false);
                            WebSoketSessionId = wssJson.GetProperty("d").GetProperty("session_id").GetString();
                            UserInfo = JsonSerializer.Deserialize<User>(wssJson.GetProperty("d").GetProperty("user").GetRawText());
                            OnReady?.Invoke(this, UserInfo);
                            break;
                        case "RESUMED":
                            OnResumed?.Invoke(this, wssJson);
                            break;
                        case "GUILD_CREATE":
                        case "GUILD_UPDATE":
                        case "GUILD_DELETE":
                            /*频道事件*/
                            OnGuildMsg?.Invoke(this, wssJson, (ActionType)Enum.Parse(typeof(ActionType), type));
                            break;
                        case "CHANNEL_CREATE":
                        case "CHANNEL_UPDATE":
                        case "CHANNEL_DELETE":
                            /*子频道事件*/
                            OnChannelMsg?.Invoke(this, wssJson, (ActionType)Enum.Parse(typeof(ActionType), type));
                            break;
                        case "GUILD_MEMBER_ADD":
                        case "GUILD_MEMBER_UPDATE":
                        case "GUILD_MEMBER_REMOVE":
                            /*频道成员事件*/
                            OnGuildMemberMsg?.Invoke(this, wssJson, (ActionType)Enum.Parse(typeof(ActionType), type));
                            break;
                        case "MESSAGE_REACTION_ADD":
                        case "MESSAGE_REACTION_REMOVE":
                            /*表情表态事件*/
                            OnMessageReaction?.Invoke(this, wssJson, (ActionType)Enum.Parse(typeof(ActionType), type));
                            break;
                        case "DIRECT_MESSAGE_CREATE":
                            /*机器人收到私信事件*/
                            OnDirectMessage?.Invoke(this, wssJson, (ActionType)Enum.Parse(typeof(ActionType), type));
                            break;
                        case "AUDIO_START":
                        case "AUDIO_FINISH":
                        case "AUDIO_ON_MIC":
                        case "AUDIO_OFF_MIC":
                            /*音频事件*/
                            OnAudioMsg?.Invoke(this, wssJson, (ActionType)Enum.Parse(typeof(ActionType), type));
                            break;
                        case "AT_MESSAGE_CREATE":
                            /*收到 @机器人 消息事件*/
                            Message message = JsonSerializer.Deserialize<Message>(wssJson.GetProperty("d").GetRawText()) ?? new();
                            string paramStr = message.Content.TrimStartString(MsgTag.UserTag(UserInfo!.Id)).Trim();
                            string command = Commands.Keys.FirstOrDefault(command => paramStr.StartsWith(command), "");
                            if (command.Length > 0)
                            {
                                Commands[command].Invoke(this, message, paramStr.TrimStartString(command).Trim());
                            }
                            else
                            {
                                OnAtMessage?.Invoke(this, message, (ActionType)Enum.Parse(typeof(ActionType), type));
                            }
                            break;
                        default:
                            Log.Debug($"[WebSocket] Unknown message received, Type={type}");
                            break;
                    }
                    break;
                // Send&Receive 客户端或服务端发送心跳
                case (int)Opcode.Heartbeat:
                    Log.Debug($"[WebSocket][Op01] {(wssJson.GetProperty("d").GetString() == null ? "Client" : "Server")} sends a heartbeat!");
                    OnHeartbeat?.Invoke(this, wssJson);
                    if (WebSocketClient.State == WebSocketState.Open)
                    {
                        await SendHeartBeatAsync().ConfigureAwait(false);
                        _ = Task.Factory.StartNew(async () =>
                        {
                            await Task.Delay(HeartbeatInterval - 1000);
                            await ExcuteCommand(JsonDocument.Parse($"{{\"op\": {(int)Opcode.Heartbeat}, \"d\": null}}").RootElement);
                        }, TaskCreationOptions.LongRunning).ConfigureAwait(false);
                    }
                    break;
                // Send 客户端发送鉴权
                case (int)Opcode.Identify:
                    Log.Debug($"[WebSocket][Op02] Client send authentication!");
                    OnIdentify?.Invoke(this, wssJson);
                    await SendIdentifyAsync();
                    break;
                // Send 客户端恢复连接
                case (int)Opcode.Resume:
                    Log.Debug($"[WebSocket][Op06] Client resumes connection!");
                    OnResume?.Invoke(this, wssJson);
                    await SendResumeAsync();
                    break;
                // Receive 服务端通知客户端重新连接
                case (int)Opcode.Reconnect:
                    Log.Info($"[WebSocket][Op07] Server asks the client to reconnect!");
                    OnReconnect?.Invoke(this, wssJson);
                    await ExcuteCommand(JsonDocument.Parse($"{{\"op\": {(int)Opcode.Resume}}}").RootElement).ConfigureAwait(false);
                    break;
                // Receive 当identify或resume的时候，如果参数有错，服务端会返回该消息
                case (int)Opcode.InvalidSession:
                    Log.Info($"[WebSocket][Op09] 客户端鉴权信息错误!");
                    OnInvalidSession?.Invoke(this, wssJson);
                    break;
                // Receive 当客户端与网关建立ws连接之后，网关下发的第一条消息
                case (int)Opcode.Hello:
                    Log.Info($"[WebSocket][Op10] Successfully connected to the gateway!");
                    OnHello?.Invoke(this, wssJson);
                    HeartbeatInterval = wssJson.GetProperty("d").GetProperty("heartbeat_interval").GetInt32();
                    await ExcuteCommand(JsonDocument.Parse($"{{\"op\": {(int)Opcode.Identify}}}").RootElement).ConfigureAwait(false);
                    break;
                // Receive 当发送心跳成功之后，就会收到该消息
                case (int)Opcode.HeartbeatACK:
                    Log.Info($"[WebSocket][Op11] HeartbeatACK Received!");
                    OnHeartbeatACK?.Invoke(this, wssJson);
                    break;
                // 未知操作码
                default:
                    Log.Warn($"[WebSocket][OpNC] Unknown Opcode: {opcode}");
                    break;
            }
        }
        #endregion

        /// <summary>
        /// 关闭机器人并释放所有占用的资源
        /// </summary>
        public static void Close()
        {
            WebSocketClient.Abort();
            WebSocketClient.Dispose();
            WebHttpClient.CancelPendingRequests();
            WebHttpClient.Dispose();
        }
        /// <summary>
        /// 启动机器人
        /// <para><em>RetryCount</em> 连接服务器失败后的重试次数</para>
        /// </summary>
        /// <param name="RetryCount">连接服务器失败后的重试次数</param>
        public async void Start(int RetryCount = 3)
        {
            await ConnectAsync(RetryCount).ConfigureAwait(false);
        }
    }

    public struct Identity
    {
        /// <summary>
        /// 机器人Id
        /// </summary>
        public string BotAppId { get; set; }
        /// <summary>
        /// 机器人Token
        /// </summary>
        public string BotToken { get; set; }
        /// <summary>
        /// 机器人密钥
        /// </summary>
        public string BotSecret { get; set; }
    }
}
