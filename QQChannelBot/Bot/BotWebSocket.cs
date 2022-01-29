using System.Buffers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using QQChannelBot.Bot.SocketEvent;
using QQChannelBot.Models;

namespace QQChannelBot.Bot
{
    public partial class BotClient
    {
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
                                    guild.APIPermissions = await GetGuildPermissions(guild.Id);
                                    Guilds[guild.Id] = guild;
                                    break;
                                case "GUILD_DELETE":
                                    Guilds.Remove(guild.Id, out _);
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
                            MessageAudited? messageAudited = d.Deserialize<MessageAudited>()!;
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

                            Log.Info($"[WebSocket][GetGuilds] 获取已加入的频道列表，分页大小：100");
                            string? guildNext = null;
                            for (int page = 1; ; ++page)
                            {
                                List<Guild>? guilds = await GetMeGuildsAsync(guildNext);
                                if (guilds == null)
                                {
                                    Log.Info($"[WebSocket][GetGuilds] 获取已加入的频道列表，第 {page:00} 页失败");
                                    break;
                                }
                                if (guilds.Count == 0)
                                {
                                    Log.Info($"[WebSocket][GetGuilds] 获取已加入的频道列表，第 {page:00} 页为空，操作结束");
                                    break;
                                }
                                Log.Info($"[WebSocket][GetGuilds] 获取已加入的频道列表，第 {page:00} 页成功，数量：{guilds.Count}");
                                Parallel.ForEach(guilds, (guild, state, i) =>
                                {
                                    guild.APIPermissions = GetGuildPermissions(guild.Id).Result;
                                    Guilds[guild.Id] = guild;
                                });
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
    }
}
