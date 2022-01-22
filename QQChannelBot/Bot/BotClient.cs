﻿using System.Buffers;
using System.Net;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using QQChannelBot.Bot.SocketEvent;
using QQChannelBot.Bot.StatusCode;
using QQChannelBot.Models;
using QQChannelBot.MsgHelper;

namespace QQChannelBot.Bot
{
    /// <summary>
    /// 机器人对象
    /// <para>
    /// 可选属性配置表：<br/>
    /// BotAccessInfo - 机器人鉴权登陆信息,见<see cref="Identity"/><br/>
    /// SadboxGuildId - 指定用于调试机器人的沙箱频道(DebugBot=true时有效)<br/>
    /// DebugBot - 指定机器人运行的模式[true:测试; false:正式]；默认值=false<br/>
    /// Info - 机器人的 <see cref="User"/> 信息(在机器人鉴权通过后更新)；默认值=null<br/>
    /// Members - 自动记录机器人在各频道内的身份组信息<br/>
    /// ReportApiError - 向前端消息发出者报告API错误[true:报告;false:静默]；默认值=false<br/>
    /// SandBox - 机器人调用API的模式[true:沙箱;false:正式]；默认值=false<br/>
    /// ApiOrigin - (只读) 获取机器人当前使用的ApiUrl<br/>
    /// Intents - 订阅频道事件,详见:<see cref="Intent"/>；默认值=(GUILDS|GUILD_MEMBERS|AT_MESSAGES|GUILD_MESSAGE_REACTIONS)<br/>
    /// Guilds - 机器人已加入的频道列表
    /// </para>
    /// </summary>
    public partial class BotClient
    {
        #region 可以监听的固定事件列表
        /// <summary>
        /// WebSocketClient连接后触发
        /// <para>等同于 WebSocket.OnOpen 事件</para>
        /// </summary>
        public event Action<BotClient>? OnWebSocketConnected;
        /// <summary>
        /// WebSocketClient关闭后触发
        /// </summary>
        public event Action<BotClient>? OnWebSocketClosed;
        /// <summary>
        /// WebSocketClient发送数据前触发
        /// </summary>
        public event Action<BotClient, string>? OnWebSoketSending;
        /// <summary>
        /// WebSocketClient收到数据后触发
        /// </summary>
        public event Action<BotClient, string>? OnWebSocketReceived;
        /// <summary>
        /// 收到服务端推送的消息时触发
        /// </summary>
        public event Action<BotClient, JsonElement>? OnDispatch;
        /// <summary>
        /// 客户端发送心跳或收到服务端推送心跳时触发
        /// </summary>
        public event Action<BotClient, JsonElement>? OnHeartbeat;
        /// <summary>
        /// 客户端发送鉴权时触发
        /// </summary>
        public event Action<BotClient, JsonElement>? OnIdentify;
        /// <summary>
        /// 客户端恢复连接时触发
        /// </summary>
        public event Action<BotClient, JsonElement>? OnResume;
        /// <summary>
        /// 服务端通知客户端重新连接时触发
        /// </summary>
        public event Action<BotClient, JsonElement>? OnReconnect;
        /// <summary>
        /// 当identify或resume的时候，参数错误的时候触发
        /// </summary>
        public event Action<BotClient, JsonElement>? OnInvalidSession;
        /// <summary>
        /// 当客户端与网关建立ws连接的时候触发
        /// </summary>
        public event Action<BotClient, JsonElement>? OnHello;
        /// <summary>
        /// 客户端发送心跳被服务端接收后触发
        /// </summary>
        public event Action<BotClient, JsonElement>? OnHeartbeatACK;
        /// <summary>
        /// 鉴权连接成功后触发
        /// <para>注:此时获取的User对象只有3个属性 {id,username,bot}</para>
        /// </summary>
        public event Action<User>? OnReady;
        /// <summary>
        /// 恢复连接成功后触发
        /// </summary>
        public event Action<BotClient, JsonElement>? OnResumed;
        /// <summary>
        /// 频道信息变更后触发
        /// <para>
        /// 机器人加入频道, 频道资料变更, 机器人退出频道<br/>
        /// BotClient - 机器人对象<br/>
        /// Guild - 频道对象<br/>
        /// string - 事件类型（GUILD_ADD | GUILD_UPDATE | GUILD_REMOVE）
        /// </para>
        /// </summary>
        public event Action<BotClient, Guild, string>? OnGuildMsg;
        /// <summary>
        /// 子频道被修改后触发
        /// <para>
        /// 子频道被创建, 子频道资料变更, 子频道被删除<br/>
        /// BotClient - 机器人对象<br/>
        /// Channel - 频道对象（没有分组Id和排序位置属性）<br/>
        /// string - 事件类型（CHANNEL_CREATE|CHANNEL_UPDATE|CHANNEL_DELETE）
        /// </para>
        /// </summary>
        public event Action<BotClient, Channel, string>? OnChannelMsg;
        /// <summary>
        /// 成员信息变更后触发
        /// <para>
        /// 成员加入, 资料变更, 移除成员<br/>
        /// BotClient - 机器人对象<br/>
        /// MemberWithGuildID - 成员对象<br/>
        /// string - 事件类型（GUILD_MEMBER_ADD | GUILD_MEMBER_UPDATE | GUILD_MEMBER_REMOVE）
        /// </para>
        /// </summary>
        public event Action<BotClient, MemberWithGuildID, string>? OnGuildMemberMsg;
        /// <summary>
        /// 修改表情表态后触发
        /// <para>
        /// 添加表情表态, 删除表情表态<br/>
        /// BotClient - 机器人对象<br/>
        /// MessageReaction - 表情表态对象<br/>
        /// string - 事件类型（MESSAGE_REACTION_ADD|MESSAGE_REACTION_REMOVE）
        /// </para>
        /// </summary>
        public event Action<BotClient, MessageReaction, string>? OnMessageReaction;
        /// <summary>
        /// 消息审核出结果后触发
        /// <para>
        /// 消息审核通过才有MessageId属性<br/>
        /// BotClient - 机器人对象<br/>
        /// MessageAudited - 消息审核对象<br/>
        /// MessageAudited.IsPassed - 消息审核是否通过
        /// </para>
        /// </summary>
        public event Action<BotClient, MessageAudited>? OnMessageAudit;
        /// <summary>
        /// 音频状态变更后触发
        /// </summary>
        public event Action<BotClient, JsonElement?>? OnAudioMsg;
        /// <summary>
        /// 频道内有人发消息就触发 (包含 @机器人 消息)
        /// <para>
        /// Sender - 发件人对象<br/>
        /// <see cref="Sender.MessageType"/> 包含消息类别（公开，AT机器人，AT全员，私聊）
        /// </para>
        /// </summary>
        public event Action<Sender>? OnMsgCreate;
        /// <summary>
        /// API调用出错时触发
        /// <para>
        /// Sender - 发件人对象（仅当调用API的主体为Sender时才有）<br/>
        /// List&lt;string&gt; - API错误信息{接口地址，请求方式，异常代码，异常原因}<br/>
        /// FreezeTime - 对访问出错的接口，暂停使用的时间
        /// </para>
        /// </summary>
        public event Action<Sender?, ApiErrorInfo>? OnApiError;
        /// <summary>
        /// 消息过滤器
        /// <para>当返回值为True时，该消息将被拦截并丢弃</para>
        /// </summary>
        public Func<Sender, bool>? MessageFilter;
        #endregion

        /// <summary>
        /// 鉴权信息
        /// <para>可在这里查询 <see href="https://bot.q.qq.com/#/developer/developer-setting">QQ机器人开发设置</see></para>
        /// </summary>
        public Identity BotAccessInfo { get; set; }
        /// <summary>
        /// 向前端指令发出者报告API错误
        /// <para>注：该属性作为 <see cref="Sender.ReportError"/> 的默认值使用</para>
        /// </summary>
        public bool ReportApiError { get; set; }
        /// <summary>
        /// 机器人用户信息
        /// </summary>
        public User Info { get; private set; } = new User();
        /// <summary>
        /// 保存机器人在各频道内的角色信息
        /// <para>
        /// string - GUILD_ID<br/>
        /// Member - 角色信息
        /// </para>
        /// </summary>
        public Dictionary<string, Member?> Members { get; private set; } = new();

        #region Http客户端配置
        /// <summary>
        /// 集中处理机器人的HTTP请求
        /// </summary>
        /// <param name="url">请求网址</param>
        /// <param name="method">请求类型(默认GET)</param>
        /// <param name="content">请求数据</param>
        /// <param name="sender">指示本次请求由谁发起的</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage?> HttpSendAsync(string url, HttpMethod? method = null, HttpContent? content = null, Sender? sender = null)
        {
            method ??= HttpMethod.Get;
            HttpRequestMessage request = new() { RequestUri = new Uri(url), Content = content, Method = method };
            request.Headers.Authorization = new("Bot", $"{BotAccessInfo.BotAppId}.{BotAccessInfo.BotToken}");
            // 捕获Http请求错误
            return await BotHttpClient.SendAsync(request, async (response, freezeTime) =>
            {
                ApiErrorInfo errInfo = new(url.TrimStart(ApiOrigin), method.Method, response.StatusCode.GetHashCode(), "此错误类型未收录!", freezeTime);
                if (response.Content.Headers.ContentType?.MediaType == "application/json")
                {
                    ApiStatus? err = JsonSerializer.Deserialize<ApiStatus>(await response.Content.ReadAsStringAsync());
                    if (err?.Code != null) errInfo.Code = err.Code;
                    if (err?.Message != null) errInfo.Detail = err.Message;
                }
                if (StatusCodes.OpenapiCode.TryGetValue(errInfo.Code, out string? value)) errInfo.Detail = value;
                string senderMaster = (sender?.Bot.Guilds.TryGetValue(sender.GuildId, out var guild) == true ? guild.Name : null) ?? sender?.Author.UserName ?? "";
                Log.Error($"[接口访问失败]{senderMaster} 代码：{errInfo.Code}，详情：{errInfo.Detail}");

                OnApiError?.Invoke(sender, errInfo);
                if (sender != null)
                {
                    if (sender.ReportError != true) Log.Info($"[接口访问失败] 机器人配置ReportError!=true，取消推送给发件人");
                    else if (sender.Message.CreateTime.AddMinutes(5) < DateTime.Now) Log.Info($"[接口访问失败] 被动可回复时间已超时，取消推送给发件人");
                    else
                    {
                        _ = Task.Run(async delegate
                        {
                            sender.ReportError = false;
                            await sender.ReplyAsync(string.Join('\n',
                                "❌接口访问失败",
                                "接口地址：" + errInfo.Path,
                                "请求方式：" + errInfo.Method,
                                "异常代码：" + errInfo.Code,
                                "异常详情：" + errInfo.Detail,
                                errInfo.FreezeTime.EndTime > DateTime.Now ? $"接口冻结：暂停使用此接口 {errInfo.FreezeTime.AddTime.Minutes}分{errInfo.FreezeTime.AddTime.Seconds}秒\n解冻时间：{errInfo.FreezeTime.EndTime:yyyy-MM-dd HH:mm:ss}" : null
                                ));
                        });
                    }
                }
                else Log.Info($"[接口访问失败] 访问接口的主体不是Sender，取消推送给发件人");
            });
        }
        /// <summary>
        /// 正式环境
        /// </summary>
        private static string ReleaseApi => "https://api.sgroup.qq.com";
        /// <summary>
        /// 沙箱环境
        /// <para>
        /// 沙箱环境只会收到测试频道的事件，且调用openapi仅能操作测试频道
        /// </para>
        /// </summary>
        private static string SandboxApi => "https://sandbox.api.sgroup.qq.com";
        /// <summary>
        /// 机器人接口域名
        /// </summary>
        public string ApiOrigin { get; init; }
        /// <summary>
        /// 缓存最后一次发送的消息
        /// </summary>
        private List<Message> StackSendMessage { get; set; } = new();
        /// <summary>
        /// 存取最后一次发送的消息
        /// <para>注：目的是为了用于撤回消息，所以自动删除5分钟以上的记录</para>
        /// </summary>
        /// <param name="msg">需要存储的msg，或者用于检索同频道的msg</param>
        /// <param name="push">fase-出栈；true-入栈</param>
        private Message? LastSendMessage(Message? msg, bool push = false)
        {
            if (msg == null) return null;
            DateTime outTime = DateTime.Now.AddMinutes(-10);
            StackSendMessage.RemoveAll(m => m.CreateTime < outTime);
            if (push) StackSendMessage.Add(msg);
            else
            {
                int stackIndex = StackSendMessage.FindLastIndex(m => m.GuildId.Equals(msg.GuildId) && m.ChannelId.Equals(msg.ChannelId));
                if (stackIndex >= 0)
                {
                    msg = StackSendMessage[stackIndex];
                    StackSendMessage.RemoveAt(stackIndex);
                }
                else msg = null;
            }
            return msg;
        }
        /// <summary>
        /// 机器人已加入的频道
        /// </summary>
        public Dictionary<string, Guild> Guilds { get; private set; } = new();
        #endregion

        #region Socket客户端配置
        /// <summary>
        /// Socket客户端
        /// </summary>
        private ClientWebSocket WebSocketClient { get; set; } = new();
        /// <summary>
        /// Socket客户端收到的最新的消息的s，如果是第一次连接，传null
        /// </summary>
        private int WebSocketLastSeq { get; set; } = 1;
        /// <summary>
        /// 此次连接所需要接收的事件
        /// <para>具体可参考 <see href="https://bot.q.qq.com/wiki/develop/api/gateway/intents.html">事件订阅</see></para>
        /// </summary>
        public Intent Intents { get; set; } = SocketEvent.Intents.Public;
        /// <summary>
        /// 会话分片信息
        /// </summary>
        private WebSocketLimit? GateLimit { get; set; }
        /// <summary>
        /// 会话分片id
        /// <para>
        /// 分片是按照频道id进行哈希的，同一个频道的信息会固定从同一个链接推送。<br/>
        /// 详见 <see href="https://bot.q.qq.com/wiki/develop/api/gateway/shard.html">Shard机制</see>
        /// </para>
        /// </summary>
        public int ShardId { get; set; } = 0;
        /// <summary>
        /// Socket客户端存储的SessionId
        /// </summary>
        private string? WebSoketSessionId { get; set; }
        /// <summary>
        /// 断线重连状态标志
        /// </summary>
        private bool IsResume { get; set; } = false;
        /// <summary>
        /// Socket心跳定时器
        /// </summary>
        private System.Timers.Timer HeartBeatTimer { get; set; } = new();
        #endregion

        /// <summary>
        /// QQ频道机器人
        /// </summary>
        /// <param name="identity">机器人鉴权信息</param>
        /// <param name="sandBoxApi">使用沙箱API</param>
        /// <param name="reportApiError">向前端用户反馈API错误</param>
        public BotClient(Identity identity, bool sandBoxApi = false, bool reportApiError = false)
        {
            BotAccessInfo = identity;
            ApiOrigin = sandBoxApi ? SandboxApi : ReleaseApi;
            ReportApiError = reportApiError;
            HeartBeatTimer.Elapsed += async (sender, e) => await ExcuteCommand(JsonDocument.Parse("{\"op\":" + (int)Opcode.Heartbeat + "}").RootElement);
        }

        #region 自定义指令注册
        /// <summary>
        /// 自定义指令前缀
        /// <para>
        /// 当机器人识别到消息的头部包含指令前缀时触发指令识别功能<br/>
        /// 默认值："/"
        /// </para>
        /// </summary>
        public string CommandPrefix { get; set; } = "/";
        /// <summary>
        /// 缓存动态注册的消息指令事件
        /// </summary>
        private Dictionary<string, Command> Commands { get; } = new();
        /// <summary>
        /// 添加消息指令
        /// <para>
        /// 注1：指令匹配忽略消息前的 @机器人 标签，并移除所有前导和尾随空白字符。<br/>
        /// 注2：被指令命中的消息不会再触发 OnAtMessage 和 OnMsgCreate 事件
        /// </para>
        /// </summary>
        /// <param name="command">指令对象</param>
        /// <param name="overwrite">指令名称重复的处理办法<para>true:替换, false:忽略</para></param>
        /// <returns></returns>
        public BotClient AddCommand(Command command, bool overwrite = false)
        {
            string cmdName = command.Name;
            if (Commands.ContainsKey(cmdName))
            {
                if (overwrite)
                {
                    Log.Warn($"[CommandManager] 指令 {cmdName} 已存在,已替换新注册的功能!");
                    Commands[cmdName] = command;
                }
                else Log.Warn($"[CommandManager] 指令 {cmdName} 已存在,已忽略新功能的注册!");
            }
            else
            {
                Log.Info($"[CommandManager] 指令 {cmdName} 已注册.");
                Commands[cmdName] = command;
            }
            return this;
        }
        /// <summary>
        /// 删除消息指令
        /// </summary>
        /// <param name="cmdName">指令名称</param>
        /// <returns></returns>
        public BotClient DelCommand(string cmdName)
        {
            if (Commands.ContainsKey(cmdName))
            {
                Commands.Remove(cmdName);
                Log.Info($"[CommandManager] 指令 {cmdName} 已删除.");
            }
            else Log.Warn($"[CommandManager] 指令 {cmdName} 不存在!");
            return this;
        }
        /// <summary>
        /// 获取所有已注册的指令
        /// </summary>
        public List<Command> GetCommands => Commands.Values.ToList();
        #endregion

        #region 用户API(机器人)
        /// <summary>
        /// 获取当前用户(机器人)信息
        /// <para>此API无需任何权限</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <returns>当前用户对象</returns>
        public async Task<User?> GetMeAsync(Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/users/@me", null, null, sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<User?>();
        }
        /// <summary>
        /// 获取当前用户(机器人)已加入频道列表
        /// <para>此API无需任何权限</para>
        /// </summary>
        /// <param name="guild_id">频道Id（作为拉取下一次列表的分页坐标使用）</param>
        /// <param name="route">数据拉取方向（true-向前查找 | false-向后查找）</param>
        /// <param name="limit">数据分页（默认每次拉取100条）</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<List<Guild>?> GetMeGuildsAsync(string? guild_id = null, bool route = false, int limit = 100, Sender? sender = null)
        {
            guild_id = string.IsNullOrWhiteSpace(guild_id) ? "" : $"&{(route ? "before" : "after")}={guild_id}";
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/users/@me/guilds?limit={limit}{guild_id}", null, null, sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<List<Guild>?>();
        }
        #endregion Pass

        #region 频道API
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
        #endregion Pass

        #region 子频道API
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
        #endregion Pass

        #region 成员API
        /// <summary>
        /// 获取频道成员详情
        /// </summary>
        /// <param name="guild_id">频道Id</param>
        /// <param name="user_id">成员用户Id</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<Member?> GetMemberAsync(string guild_id, string user_id, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/guilds/{guild_id}/members/{user_id}", null, null, sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<Member?>();
        }
        /// <summary>
        /// 获取频道成员列表（仅私域可用）
        /// <para>
        /// guild_id - 频道Id<br/>
        /// limit - 分页大小1-1000（默认值10）<br/>
        /// after - 上次回包中最后一个Member的用户ID，首次请求填0
        /// </para>
        /// </summary>
        /// <param name="guild_id">频道Id</param>
        /// <param name="limit">分页大小1-1000（默认值10）</param>
        /// <param name="after">上次回包中最后一个Member的用户ID，首次请求填"0"</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<List<Member>?> GetGuildMembersAsync(string guild_id, int limit = 10, string? after = null, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/guilds/{guild_id}/members?limit={limit}&after={after ?? "0"}", null, null, sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<List<Member>?>();
        }
        /// <summary>
        /// 删除指定频道成员（仅私域可用）
        /// </summary>
        /// <param name="guild_id">频道Id</param>
        /// <param name="user_id">用户Id</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<bool> DeleteGuildMemberAsync(string guild_id, string user_id, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/guilds/{guild_id}/members/{user_id}", HttpMethod.Delete, null, sender);
            return respone?.IsSuccessStatusCode ?? false;
        }
        #endregion Pass

        #region 频道身份组API
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
        #endregion Pass

        #region 子频道权限API
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
        #endregion Pass

        #region 消息API
        /// <summary>
        /// 获取消息列表（待验证）
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
            return LastSendMessage(result, true);
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
            return respone?.StatusCode == HttpStatusCode.OK;
        }
        /// <summary>
        /// 撤回机器人在当前子频道发出的最后一条消息
        /// <para>
        /// 需要传入指令发出者的消息对象<br/>
        /// 用于检索指令发出者所在频道信息
        /// </para>
        /// </summary>
        /// <param name="senderMessage">发件人消息对象</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<bool> DeleteLastMessageAsync(Message senderMessage, Sender? sender = null)
        {
            Message? lastMessage = LastSendMessage(senderMessage);
            return lastMessage != null && await DeleteMessageAsync(lastMessage.ChannelId, lastMessage.Id, sender);
        }
        #endregion Pass

        #region 私信API
        /// <summary>
        /// 创建私信会话
        /// </summary>
        /// <param name="recipient_id">接收者Id</param>
        /// <param name="source_guild_id">源频道Id</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<DirectMessageSource?> CreateDMSAsync(string recipient_id, string source_guild_id, Sender sender)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/users/@me/dms", HttpMethod.Post, JsonContent.Create(new { recipient_id, source_guild_id }), sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<DirectMessageSource?>();
        }
        /// <summary>
        /// 发送私信
        /// <para>用于发送私信消息，前提是已经创建了私信会话。</para>
        /// </summary>
        /// <param name="guild_id">私信频道Id</param>
        /// <param name="message">消息对象</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<Message?> SendPMAsync(string guild_id, MessageToCreate message, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/dms/{guild_id}/messages", HttpMethod.Post, JsonContent.Create(message), sender);
            Message? result = respone == null ? null : await respone.Content.ReadFromJsonAsync<Message?>();
            return LastSendMessage(result, true);
        }

        #endregion

        #region 禁言API
        /// <summary>
        /// 频道全局禁言
        /// <para>
        /// muteTime - 禁言时间：<br/>
        /// mute_end_timestamp 禁言到期时间戳，绝对时间戳，单位：秒<br/>
        /// mute_seconds 禁言多少秒
        /// </para>
        /// </summary>
        /// <param name="guild_id">频道Id</param>
        /// <param name="muteTime">禁言模式</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<bool> MuteGuildAsync(string guild_id, MuteTime muteTime, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/guilds/{guild_id}/mute", HttpMethod.Patch, JsonContent.Create(muteTime), sender);
            return respone?.IsSuccessStatusCode ?? false;
        }
        /// <summary>
        /// 频道指定成员禁言
        /// <para>
        /// muteTime - 禁言时间：<br/>
        /// mute_end_timestamp 禁言到期时间戳，绝对时间戳，单位：秒<br/>
        /// mute_seconds 禁言多少秒
        /// </para>
        /// </summary>
        /// <param name="guild_id">频道Id</param>
        /// <param name="user_id">成员Id</param>
        /// <param name="muteTime">禁言时间</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<bool> MuteMemberAsync(string guild_id, string user_id, MuteTime muteTime, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/guilds/{guild_id}/members/{user_id}/mute", HttpMethod.Patch, JsonContent.Create(muteTime), sender);
            return respone?.IsSuccessStatusCode ?? false;
        }
        #endregion Pass

        #region 公告API
        /// <summary>
        /// 创建频道全局公告
        /// </summary>
        /// <param name="guild_id">频道Id</param>
        /// <param name="channel_id">消息所在子频道Id</param>
        /// <param name="message_id">消息Id</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<Announces?> CreateAnnouncesGlobalAsync(string guild_id, string channel_id, string message_id, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/guilds/{guild_id}/announces", HttpMethod.Post, channel_id == null ? null : JsonContent.Create(new { channel_id, message_id }), sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<Announces?>();
        }
        /// <summary>
        /// 创建频道全局公告
        /// </summary>
        /// <param name="msg">用于创建公告的消息对象
        /// <para><em>注：必须是已发送消息回传的Message对象</em></para>
        /// </param>
        /// <returns></returns>
        public async Task<Announces?> CreateAnnouncesGlobalAsync(Message msg) => await CreateAnnouncesGlobalAsync(msg.GuildId, msg.ChannelId, msg.Id);

        /// <summary>
        /// 删除频道全局公告
        /// </summary>
        /// <param name="guild_id">频道Id</param>
        /// <param name="message_id">用于创建公告的消息Id</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAnnouncesGlobalAsync(string guild_id, string message_id = "all", Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/guilds/{guild_id}/announces/{message_id}", HttpMethod.Delete, null, sender);
            return respone?.IsSuccessStatusCode ?? false;
        }
        /// <summary>
        /// 创建子频道公告
        /// </summary>
        /// <param name="channel_id">子频道Id</param>
        /// <param name="message_id">消息Id</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<Announces?> CreateAnnouncesAsync(string channel_id, string message_id, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/announces", HttpMethod.Post, channel_id == null ? null : JsonContent.Create(new { message_id }), sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<Announces?>();
        }
        /// <summary>
        /// 创建子频道公告
        /// </summary>
        /// <param name="msg">用于创建公告的消息对象
        /// <para><em>注：必须是已发送消息回传的Message对象</em></para>
        /// </param>
        /// <returns></returns>
        public async Task<Announces?> CreateAnnouncesAsync(Message msg) => await CreateAnnouncesAsync(msg.ChannelId, msg.Id);
        /// <summary>
        /// 删除子频道公告
        /// </summary>
        /// <param name="channel_id">子频道Id</param>
        /// <param name="message_id">用于创建公告的消息Id</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAnnouncesAsync(string channel_id, string message_id = "all", Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/announces/{message_id}", HttpMethod.Delete, null, sender);
            return respone?.IsSuccessStatusCode ?? false;
        }
        #endregion Pass

        #region 日程API
        /// <summary>
        /// 获取频道日程列表
        /// <para>
        /// 获取某个日程子频道里中当天的日程列表; <br/>
        /// 若带了参数 since，则返回结束时间在 since 之后的日程列表; <br/>
        /// 若未带参数 since，则默认返回当天的日程列表。
        /// </para>
        /// </summary>
        /// <param name="channel_id">日程子频道Id</param>
        /// <param name="since">筛选日程开始时间（默认为当日全天）</param>
        /// <param name="sender"></param>
        /// <returns>List&lt;Schedule&gt;?</returns>
        public async Task<List<Schedule>?> GetSchedulesAsync(string channel_id, DateTime? since = null, Sender? sender = null)
        {
            string param = since == null ? "" : $"?since={new DateTimeOffset(since.Value).ToUnixTimeMilliseconds()}";
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/schedules{param}", null, null, sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<List<Schedule>?>();
        }
        /// <summary>
        /// 获取单个日程信息
        /// </summary>
        /// <param name="channel_id">日程子频道Id</param>
        /// <param name="schedule_id">日程Id</param>
        /// <param name="sender"></param>
        /// <returns>目标 Schedule 对象</returns>
        public async Task<Schedule?> GetScheduleAsync(string channel_id, string schedule_id, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/schedules/{schedule_id}", null, null, sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<Schedule?>();
        }
        /// <summary>
        /// 创建日程
        /// <para>
        /// 要求操作人具有"管理频道"的权限，如果是机器人，则需要将机器人设置为管理员。<br/>
        /// 创建成功后，返回创建成功的日程对象。<br/>
        /// 日程开始时间必须大于当前时间。
        /// </para>
        /// </summary>
        /// <param name="channel_id">日程子频道Id</param>
        /// <param name="schedule">新的日程对象，不需要带Id</param>
        /// <param name="sender"></param>
        /// <returns>新创建的 Schedule 对象</returns>
        public async Task<Schedule?> CreateScheduleAsync(string channel_id, Schedule schedule, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/schedules", HttpMethod.Post, JsonContent.Create(new { schedule }), sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<Schedule?>();
        }
        /// <summary>
        /// 修改日程
        /// <para>
        /// 要求操作人具有"管理频道"的权限，如果是机器人，则需要将机器人设置为管理员。<br/>
        /// 修改成功后，返回修改后的日程对象。
        /// </para>
        /// </summary>
        /// <param name="channel_id">日程子频道Id</param>
        /// <param name="schedule">修改后的日程对象</param>
        /// <param name="sender"></param>
        /// <returns>修改后的 Schedule 对象</returns>
        public async Task<Schedule?> EditScheduleAsync(string channel_id, Schedule schedule, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/schedules/{schedule.Id}", HttpMethod.Patch, JsonContent.Create(new { schedule }), sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<Schedule?>();
        }
        /// <summary>
        /// 删除日程
        /// <para>
        /// 要求操作人具有"管理频道"的权限，如果是机器人，则需要将机器人设置为管理员。
        /// </para>
        /// </summary>
        /// <param name="channel_id">日程子频道Id</param>
        /// <param name="schedule">日程对象
        /// <para>这里是为了获取日程Id，为了防错设计为传递日程对象</para></param>
        /// <param name="sender"></param>
        /// <returns>HTTP 状态码 204</returns>
        public async Task<bool> DeleteScheduleAsync(string channel_id, Schedule schedule, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/schedules/{schedule.Id}", HttpMethod.Delete, null, sender);
            return respone?.IsSuccessStatusCode ?? false;
        }
        #endregion Pass

        #region 音频API
        /// <summary>
        /// 音频控制
        /// </summary>
        /// <param name="channel_id">子频道Id</param>
        /// <param name="audioControl">音频对象</param>
        /// <param name="sender"></param>
        /// <returns></returns>
        public async Task<Message?> AudioControlAsync(string channel_id, AudioControl audioControl, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/audio", HttpMethod.Post, JsonContent.Create(audioControl), sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<Message?>();
        }
        #endregion

        #region WebSocketAPI 
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
        #endregion

        #region WebSocket
        /// <summary>
        /// 建立到服务器的连接
        /// <para><em>RetryCount</em> 连接失败后允许的重试次数</para>
        /// </summary>
        /// <param name="RetryCount">连接失败后允许的重试次数</param>
        /// <returns></returns>
        public async Task ConnectAsync(int RetryCount)
        {
            int retryEndTime = 10;
            int retryAddTime = 10;
            while (RetryCount-- > 0)
            {
                try
                {
                    GateLimit = await GetWssUrlWithShared();
                    if (Uri.TryCreate(GateLimit?.Url, UriKind.Absolute, out Uri? webSocketUri))
                    {
                        if (WebSocketClient.State != WebSocketState.Open && WebSocketClient.State != WebSocketState.Connecting)
                        {
                            await WebSocketClient.ConnectAsync(webSocketUri, CancellationToken.None);
                            OnWebSocketConnected?.Invoke(this);
                            _ = ReceiveAsync();
                        }
                        break;
                    }
                    Log.Error($"[WebSocket][Connect] 使用网关地址<{GateLimit?.Url}> 建立连接失败！");
                }
                catch (Exception e)
                {
                    Log.Error($"[WebSocket][Connect] {e.Message} | Status：{WebSocketClient.CloseStatus} | Description：{WebSocketClient.CloseStatusDescription}");
                }
                if (RetryCount > 0)
                {
                    for (int i = retryEndTime; 0 < i; --i)
                    {
                        Log.Info($"[WebSocket] {i} 秒后再次尝试连接（剩余重试次数：${RetryCount}）...");
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                    retryEndTime += retryAddTime;
                }
                else
                {
                    Log.Error($"[WebSocket] 重连次数已耗尽，无法与频道服务器建立连接！");
                }
            }
        }
        /// <summary>
        /// 鉴权连接
        /// </summary>
        /// <returns></returns>
        private async Task SendIdentifyAsync()
        {
            var data = new
            {
                op = Opcode.Identify,
                d = new
                {
                    token = $"Bot {BotAccessInfo.BotAppId}.{BotAccessInfo.BotToken}",
                    intents = Intents.GetHashCode(),
                    shared = new[] { ShardId % (GateLimit?.Shards ?? 1), GateLimit?.Shards ?? 1 }
                }
            };
            string sendMsg = JsonSerializer.Serialize(data);
            Log.Debug("[WebSocket][SendIdentify] " + Regex.Replace(sendMsg, @"(?<=Bot\s+)[^""]+", (m) => Regex.Replace(m.Groups[0].Value, @"[^\.]", "*"))); // 敏感信息脱敏处理
            await WebSocketSendAsync(sendMsg, WebSocketMessageType.Text, true);
        }
        /// <summary>
        /// 发送心跳
        /// </summary>
        /// <returns></returns>
        private async Task SendHeartBeatAsync()
        {
            if (WebSocketClient.State == WebSocketState.Open)
            {
                string sendMsg = "{\"op\": 1, \"d\":" + WebSocketLastSeq + "}";
                Log.Debug($"[WebSocket][SendHeartbeat] {sendMsg}");
                await WebSocketSendAsync(sendMsg, WebSocketMessageType.Text, true);
            }
            else Log.Error($"[WebSocket][Heartbeat] 未建立连接！");
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
                        seq = WebSocketLastSeq
                    }
                };
                string sendMsg = JsonSerializer.Serialize(data);
                Log.Debug($"[WebSocket][SendResume] {sendMsg}");
                await WebSocketSendAsync(sendMsg, WebSocketMessageType.Text, true);
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
            OnWebSoketSending?.Invoke(this, data);
            await WebSocketClient.SendAsync(Encoding.UTF8.GetBytes(data), msgType, endOfMsg, cancelToken ?? CancellationToken.None);
        }
        /// <summary>
        /// WebSocket接收服务端数据
        /// </summary>
        /// <returns></returns>
        private async Task ReceiveAsync()
        {
            while (WebSocketClient.State == WebSocketState.Open)
            {
                try
                {
                    using IMemoryOwner<byte> memory = MemoryPool<byte>.Shared.Rent(1024 * 64);
                    ValueWebSocketReceiveResult result = await WebSocketClient.ReceiveAsync(memory.Memory, CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        JsonElement json = JsonDocument.Parse(memory.Memory[..result.Count]).RootElement;
                        OnWebSocketReceived?.Invoke(this, json.GetRawText());
                        await ExcuteCommand(json);
                        continue;
                    }
                    Log.Info($"[WebSocket][Receive] SocketType：{result.MessageType} | Status：{WebSocketClient.CloseStatus}");
                }
                catch (Exception e)
                {
                    Log.Error($"[WebSocket][Receive] {e.Message} | Status：{WebSocketClient.CloseStatus}{Environment.NewLine}");
                }
                // 关闭代码4009表示频道服务器要求的重连，可以进行Resume
                if (WebSocketClient.CloseStatus.GetHashCode() == 4009) IsResume = true;
                WebSocketClient.Abort();
                break;
            }
            if (HeartBeatTimer.Enabled) HeartBeatTimer.Enabled = false;
            OnWebSocketClosed?.Invoke(this);
            Log.Warn($"[WebSocket] 重新建立到服务器的连接...");
            await Task.Delay(TimeSpan.FromSeconds(1));
            WebSocketClient = new();
            await ConnectAsync(30);
        }
        /// <summary>
        /// 根据收到的数据分析用途
        /// </summary>
        /// <param name="wssJson">Wss接收的数据</param>
        /// <returns></returns>
        private async Task ExcuteCommand(JsonElement wssJson)
        {
            Opcode opcode = (Opcode)wssJson.GetProperty("op").GetInt32();
            switch (opcode)
            {
                // Receive 服务端进行消息推送
                case Opcode.Dispatch:
                    OnDispatch?.Invoke(this, wssJson);
                    WebSocketLastSeq = wssJson.GetProperty("s").GetInt32();
                    if (!wssJson.TryGetProperty("t", out JsonElement t) || !wssJson.TryGetProperty("d", out JsonElement d))
                    {
                        Log.Warn($"[WebSocket][Op00][Dispatch] {wssJson.GetRawText()}");
                        break;
                    }
                    string data = d.GetRawText();
                    string? type = t.GetString();
                    switch (type)
                    {
                        case "DIRECT_MESSAGE_CREATE":   // 机器人收到私信事件
                        case "AT_MESSAGE_CREATE":       // 收到 @机器人 消息事件
                        case "MESSAGE_CREATE":          // 频道内有人发言(仅私域)
                            Message? message = d.Deserialize<Message>();
                            if (message == null)
                            {
                                Log.Warn($"[WebSocket][{type}] {data}");
                                return;
                            }
                            Log.Debug($"[WebSocket][{type}] {data}");
                            _ = MessageCenter(message, type); // 处理消息，不需要等待结果
                            break;
                        case "GUILD_CREATE":
                        case "GUILD_UPDATE":
                        case "GUILD_DELETE":
                            /*频道事件*/
                            Log.Debug($"[WebSocket][{type}] {data}");
                            Guild guild = d.Deserialize<Guild>()!;
                            switch (type)
                            {
                                case "GUILD_CREATE":
                                case "GUILD_UPDATE":
                                    Guilds[guild.Id] = guild;
                                    break;
                                case "GUILD_DELETE":
                                    Guilds.Remove(guild.Id);
                                    break;
                            }
                            OnGuildMsg?.Invoke(this, guild, type);
                            break;
                        case "CHANNEL_CREATE":
                        case "CHANNEL_UPDATE":
                        case "CHANNEL_DELETE":
                            /*子频道事件*/
                            Log.Debug($"[WebSocket][{type}] {data}");
                            Channel channel = d.Deserialize<Channel>()!;
                            OnChannelMsg?.Invoke(this, channel, type);
                            break;
                        case "GUILD_MEMBER_ADD":
                        case "GUILD_MEMBER_UPDATE":
                        case "GUILD_MEMBER_REMOVE":
                            /*频道成员事件*/
                            Log.Debug($"[WebSocket][{type}] {data}");
                            MemberWithGuildID memberWithGuild = d.Deserialize<MemberWithGuildID>()!;
                            OnGuildMemberMsg?.Invoke(this, memberWithGuild, type);
                            break;
                        case "MESSAGE_REACTION_ADD":
                        case "MESSAGE_REACTION_REMOVE":
                            /*表情表态事件*/
                            Log.Debug($"[WebSocket][{type}] {data}");
                            MessageReaction messageReaction = d.Deserialize<MessageReaction>()!;
                            OnMessageReaction?.Invoke(this, messageReaction, type);
                            break;
                        case "MESSAGE_AUDIT_PASS":
                        case "MESSAGE_AUDIT_REJECT":
                            /*消息审核事件*/
                            Log.Info($"[WebSocket][{type}] {data}");
                            MessageAudited messageAudited = d.Deserialize<MessageAudited>()!;
                            messageAudited.IsPassed = type == "MESSAGE_AUDIT_PASS";
                            OnMessageAudit?.Invoke(this, messageAudited);
                            break;
                        case "AUDIO_START":
                        case "AUDIO_FINISH":
                        case "AUDIO_ON_MIC":
                        case "AUDIO_OFF_MIC":
                            /*音频事件*/
                            Log.Info($"[WebSocket][{type}] {data}");
                            OnAudioMsg?.Invoke(this, wssJson);
                            break;
                        case "RESUMED":
                            Log.Info($"[WebSocket][Op00][RESUMED] 已恢复与服务器的连接");
                            await ExcuteCommand(JsonDocument.Parse("{\"op\":" + (int)Opcode.Heartbeat + "}").RootElement);
                            OnResumed?.Invoke(this, d);
                            break;
                        case "READY":
                            Log.Debug($"[WebSocket][READY] {data}");
                            Log.Info($"[WebSocket][Op00] 服务端 鉴权成功");
                            await ExcuteCommand(JsonDocument.Parse("{\"op\":" + (int)Opcode.Heartbeat + "}").RootElement);
                            WebSoketSessionId = d.GetProperty("session_id").GetString();

                            Log.Info($"[WebSocket][GetGuilds] 获取机器人已加入的频道列表...");
                            string? guildNext = null;
                            while (true)
                            {
                                List<Guild>? guilds = await GetMeGuildsAsync(guildNext);
                                if (guilds?.Any() != true) break;
                                guilds.ForEach(guild => Guilds[guild.Id] = guild);
                                if (guilds.Count < 100) break;
                                guildNext = guilds.Last().Id;
                            }
                            Log.Info($"[WebSocket][GetGuilds] 机器人已加入 {Guilds.Count} 个频道");

                            Info = d.GetProperty("user").Deserialize<User>()!;
                            Info.Avatar = (await GetMeAsync())?.Avatar;
                            OnReady?.Invoke(Info);
                            break;
                        default:
                            Log.Warn($"[WebSocket][{type}] 未知事件");
                            break;
                    }
                    break;
                // Send&Receive 客户端或服务端发送心跳
                case Opcode.Heartbeat:
                    Log.Debug($"[WebSocket][Op01] {(wssJson.Get("d") == null ? "客户端" : "服务器")} 发送心跳包");
                    OnHeartbeat?.Invoke(this, wssJson);
                    await SendHeartBeatAsync();
                    HeartBeatTimer.Enabled = true;
                    break;
                // Send 客户端发送鉴权
                case Opcode.Identify:
                    Log.Info($"[WebSocket][Op02] 客户端 发起鉴权");
                    OnIdentify?.Invoke(this, wssJson);
                    await SendIdentifyAsync();
                    break;
                // Send 客户端恢复连接
                case Opcode.Resume:
                    Log.Info($"[WebSocket][Op06] 客户端 尝试恢复连接..");
                    IsResume = false;
                    OnResume?.Invoke(this, wssJson);
                    await SendResumeAsync();
                    break;
                // Receive 服务端通知客户端重新连接
                case Opcode.Reconnect:
                    Log.Info($"[WebSocket][Op07] 服务器 要求客户端重连");
                    OnReconnect?.Invoke(this, wssJson);
                    break;
                // Receive 当identify或resume的时候，如果参数有错，服务端会返回该消息
                case Opcode.InvalidSession:
                    Log.Warn($"[WebSocket][Op09] 客户端鉴权信息错误");
                    OnInvalidSession?.Invoke(this, wssJson);
                    break;
                // Receive 当客户端与网关建立ws连接之后，网关下发的第一条消息
                case Opcode.Hello:
                    Log.Info($"[WebSocket][Op10][成功与网关建立连接] {wssJson.GetRawText()}");
                    OnHello?.Invoke(this, wssJson);
                    int heartbeat_interval = wssJson.Get("d")?.Get("heartbeat_interval")?.GetInt32() ?? 30000;
                    HeartBeatTimer.Interval = heartbeat_interval < 30000 ? heartbeat_interval : 30000;  // 设置心跳时间为30s
                    await ExcuteCommand(JsonDocument.Parse("{\"op\":" + (int)(IsResume ? Opcode.Resume : Opcode.Identify) + "}").RootElement);
                    break;
                // Receive 当发送心跳成功之后，就会收到该消息
                case Opcode.HeartbeatACK:
                    Log.Debug($"[WebSocket][Op11] 服务器 收到心跳包");
                    OnHeartbeatACK?.Invoke(this, wssJson);
                    break;
                // 未知操作码
                default:
                    Log.Warn($"[WebSocket][OpNC] 未知操作码: {opcode}");
                    break;
            }
        }
        #endregion

        /// <summary>
        /// 关闭机器人并释放所有占用的资源
        /// </summary>
        public void Close()
        {
            WebSocketClient.Abort();
        }
        /// <summary>
        /// 启动机器人
        /// <para><em>RetryCount</em> 连接服务器失败后的重试次数</para>
        /// </summary>
        /// <param name="RetryCount">连接服务器失败后的重试次数</param>
        public async void Start(int RetryCount = 3)
        {
            await ConnectAsync(RetryCount);
        }
        /// <summary>
        /// 上帝ID
        /// <para>仅用于测试,方便验证权限功能</para>
        /// </summary>
        public string GodId { get; set; } = "15524401336961673551";
        /// <summary>
        /// 集中处理聊天消息
        /// </summary>
        /// <param name="message">消息对象</param>
        /// <param name="type">消息类型
        /// <para>
        /// DIRECT_MESSAGE_CREATE - 私信<br/>
        /// AT_MESSAGE_CREATE - 频道内 @机器人<br/>
        /// MESSAGE_CREATE - 频道内任意消息(仅私域支持)<br/>
        /// </para></param>
        /// <returns></returns>
        private async Task MessageCenter(Message message, string type)
        {
            // 记录Sender信息
            Sender sender = new(message, this);
            // 识别私聊消息
            if (message.DirectMessage) sender.MessageType = MessageType.Private;
            // 识别AT全员消息
            else if (message.MentionEveryone) sender.MessageType = MessageType.AtAll;
            // 识别AT机器人消息
            else if (message.Mentions?.Any(user => user.Id == Info.Id) == true) sender.MessageType = MessageType.AtMe;
            // 记录机器人在当前频道下的身份组信息
            if ((sender.MessageType != MessageType.Private) && !Members.ContainsKey(message.GuildId))
            {
                Members[message.GuildId] = await GetMemberAsync(message.GuildId, Info.Id);
            }
            // 调用消息拦截器
            if (MessageFilter?.Invoke(sender) == true) return;
            // 若已经启用全局消息接收，将不单独响应 AT_MESSAGES 事件，否则会造成重复响应。
            if (Intents.HasFlag(Intent.MESSAGE_CREATE) && type.StartsWith("A")) return;
            // 预处理收到的数据
            string content = message.Content.Trim().TrimStart(Info.Tag()).TrimStart();
            // 识别指令
            bool hasCommand = content.StartsWith(CommandPrefix);
            content = content.TrimStart(CommandPrefix).TrimStart();
            if ((hasCommand || (sender.MessageType == MessageType.AtMe) || (sender.MessageType == MessageType.Private)) && (content.Length > 0))
            {
                // 在新的线程上输出日志信息
                _ = Task.Run(() =>
                {
                    string msgContent = Regex.Replace(message.Content, @"<@!\d+>", m => message.Mentions!.Find(user => user.Tag() == m.Groups[0].Value)?.UserName.Insert(0, "@") ?? m.Value);
                    string senderMaster = (sender.Bot.Guilds.TryGetValue(sender.GuildId, out var guild) ? guild.Name : null) ?? sender.Author.UserName;
                    Log.Info($"[{senderMaster}][{message.Author.UserName}] {msgContent}");
                });
                // 并行遍历指令列表，提升效率
                ParallelLoopResult result = Parallel.ForEach(Commands.Values, (cmd, state, i) =>
                {
                    Match cmdMatch = cmd.Rule.Match(content);
                    if (!cmdMatch.Success) return;
                    content = content.TrimStart(cmdMatch.Groups[0].Value).TrimStart();
                    if (cmd.NeedAdmin && !(message.Member.Roles.Any(r => "24".Contains(r)) || message.Author.Id.Equals(GodId)))
                    {
                        if (sender.MessageType == MessageType.AtMe) _ = sender.ReplyAsync($"{message.Author.Tag()} 你无权使用该命令！");
                        else return;
                    }
                    else cmd.CallBack?.Invoke(sender, content);
                    state.Break();
                });
                if (!result.IsCompleted) return;
            }
            // 触发Message到达事件
            OnMsgCreate?.Invoke(sender);
        }
        /// <summary>
        /// 返回SDK相关信息
        /// <para>
        /// 框架名称_版本号<br/>
        /// 代码仓库地址<br/>
        /// 版权信息<br/>
        /// <em>作者夹带的一点私货</em>
        /// </para>
        /// </summary>
        public static string SDK => $"{InfoSDK.Name}_{InfoSDK.Version}\n{InfoSDK.GitHTTPS.Replace('.', '。')}\n{InfoSDK.Copyright}";
    }
}