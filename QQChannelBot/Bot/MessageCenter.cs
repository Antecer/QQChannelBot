using System.Diagnostics;
using System.Text.RegularExpressions;
using QQChannelBot.Bot.SocketEvent;
using QQChannelBot.Models;

namespace QQChannelBot.Bot
{
    public partial class BotClient
    {
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
            // 识别消息类型（私聊，AT全员，AT机器人）
            if (message.DirectMessage) sender.MessageType = MessageType.Private;
            else if (message.MentionEveryone) sender.MessageType = MessageType.AtAll;
            else if (message.Mentions?.Any(user => user.Id == Info.Id) == true) sender.MessageType = MessageType.AtMe;
            // 记录机器人在当前频道下的身份组信息
            if ((sender.MessageType != MessageType.Private) && !Members.ContainsKey(message.GuildId))
            {
                Members[message.GuildId] = await GetMemberAsync(message.GuildId, Info.Id);
            }
            // 若已经启用全局消息接收，将不单独响应 AT_MESSAGES 事件，否则会造成重复响应。
            if (Intents.HasFlag(Intent.MESSAGE_CREATE) && type.StartsWith("A")) return;
            // 调用消息拦截器
            if (MessageFilter?.Invoke(sender) == true) return;
            // 记录收到的消息
            LastMessage(message, true);
            // 预判收到的消息
            string content = message.Content.Trim().TrimStart(Info.Tag).TrimStart();
            // 识别指令
            bool hasCommand = content.StartsWith(CommandPrefix);
            content = content.TrimStart(CommandPrefix).TrimStart();
            if ((hasCommand || (sender.MessageType == MessageType.AtMe) || (sender.MessageType == MessageType.Private)) && (content.Length > 0))
            {
                // 在新的线程上输出日志信息
                _ = Task.Run(() =>
                {
                    string msgContent = Regex.Replace(message.Content, @"<@!\d+>", m => message.Mentions?.Find(user => user.Tag == m.Groups[0].Value)?.UserName.Insert(0, "@") ?? m.Value);
                    string senderMaster = (sender.Bot.Guilds.TryGetValue(sender.GuildId, out var guild) ? guild.Name : null) ?? sender.Author.UserName;
                    Log.Info($"[{senderMaster}][{message.Author.UserName}] {msgContent.Replace("\xA0", " ")}"); // 替换 \xA0 字符为普通空格
                });
                // 并行遍历指令列表，提升效率
                Command? selectedCMD = Commands.Values.AsParallel().FirstOrDefault(cmd =>
                {
                    Match? cmdMatch = cmd?.Rule.Match(content);
                    if (cmdMatch?.Success != true) return false;
                    content = content.TrimStart(cmdMatch.Groups[0].Value).TrimStart();
                    return true;
                }, null);
                if (selectedCMD != null)
                {
                    if (selectedCMD.NeedAdmin && !(message.Member.Roles.Any(r => "24".Contains(r)) || message.Author.Id.Equals(GodId)))
                    {
                        if (sender.MessageType == MessageType.AtMe) _ = sender.ReplyAsync($"{message.Author.Tag} 你无权使用该命令！");
                        else return;
                    }
                    else selectedCMD.CallBack?.Invoke(sender, content);
                    return;
                }
            }

            // 触发Message到达事件
            OnMsgCreate?.Invoke(sender);
        }
    }
}
