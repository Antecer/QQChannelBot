using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using QQChannelBot.Models;

namespace QQChannelBot.Bot
{
    /// <summary>
    /// 发件人对象
    /// </summary>
    public class Sender
    {
        /// <summary>
        /// Sender构造器
        /// </summary>
        /// <param name="message">收到的消息对象</param>
        /// <param name="bot">收到消息的机器人</param>
        /// <param name="reportError">向发件人报告API调用失败的错误信息</param>
        public Sender(Message message, BotClient bot, bool reportError)
        {
            Bot = bot;
            Message = message;
            ReportError = reportError;
        }
        /// <summary>
        /// 收到的消息对象
        /// <para>若没有执行对象，可为空</para>
        /// </summary>
        public Message Message { get; init; }
        /// <summary>
        /// 收到消息的机器人
        /// </summary>
        public BotClient Bot { get; init; }
        /// <summary>
        /// 发件人的用户信息
        /// </summary>
        public User Author => Message.Author;
        /// <summary>
        /// 发件人的成员信息
        /// </summary>
        public Member Member => Message.Member;
        /// <summary>
        /// 向发件人报告API调用失败的错误信息
        /// </summary>
        public bool ReportError { get; set; }
        /// <summary>
        /// 回复发件人
        /// </summary>
        /// <param name="msg">MessageToCreate消息构造对象(或其扩展对象)</param>
        /// <returns></returns>
        public async Task<Message?> ReplyAsync(MessageToCreate msg)
        {
            msg.Id = Message.Id;
            return await Bot.SendMessageAsync(Message.ChannelId, msg, this);
        }
        /// <summary>
        /// 回复发件人
        /// </summary>
        /// <param name="msg">文字消息内容</param>
        /// <returns></returns>
        public async Task<Message?> ReplyAsync(string msg)
        {
            return await ReplyAsync(new MessageToCreate() { Content = msg });
        }
    }
}
