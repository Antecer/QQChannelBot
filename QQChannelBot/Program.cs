using QQChannelBot.BotApi;
using QQChannelBot.Models;
using QQChannelBot.MsgHelper;

namespace QQChannelBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.LogLevel = LogLevel.Debug;  // 设置日志等级

            /*为了不暴露测试用的机器人信息，这里从文件读取鉴权信息*/
            #region 从文件读取鉴权信息
            string[] tokenLines = File.ReadAllLines("token.ini");
            Dictionary<string, string> credential = new();
            foreach (string s in tokenLines)
            {
                if (s.Contains('='))
                {
                    string[] kv = s.Split('=');
                    credential[kv[0].Trim()] = kv[1].Trim(new[] { ' ', '"' });
                }
            }
            #endregion

            // 创建机器人
            ChannelBot bot = new(new()
            {
                BotAppId = credential["BotAppId"],
                BotToken = credential["BotToken"],
                BotSecret = credential["BotSecret"]
            }, false);

            // 订阅 ReadyAction 事件，这里根据机器人信息修改控制台标题
            bot.ReadyAction += (sender, e) => { Console.Title = $"QCBot: {e?.UserName}<{e?.Id}>"; };

            // 注册自定义命令，这里是让机器人复读用户的消息
            bot.AddCommand("复读", async (sender, e, msg) =>
            {
                await sender.SendMessageAsync(e.ChannelId, new MsgNormal(e.Id)
                {
                    Content = msg
                }.Body);
            });

            // 注册自定义命令，这里测试embed消息
            bot.AddCommand("UserInfo", async (sender, e, msg) =>
            {
                MsgEmbed ReplyEmbed = new(e.Id)
                {
                    Content = MsgTag.UserTag(e.Author.Id),
                    Title = e.Author.UserName,
                    Thumbnail = e.Author?.Avatar,
                    Fields = new List<MessageEmbedField>()
                        {
                            new MessageEmbedField(){Name = $"账户类别：{(e.Author!.Bot? "机器人" : "人类")}"},
                            new MessageEmbedField(){Name = $"加入时间：{e.Member?.JoinedAt}"},
                            new MessageEmbedField(){Name = $"角色分类: {string.Join("、" ,e.Member?.Roles!.Select(r=>DefaultRoles.GetName(r)) ?? new List<string>())}"}
                        }
                };
                await sender.SendMessageAsync(e.ChannelId, ReplyEmbed.Body);
            });

            // 订阅 AtMessageAction 事件，处理所有收到的 @机器人 消息
            // 注1：被 AddCommand 命令匹配的消息不会出现在这里
            // 注2：若要接收服务器推送的所有消息，请订阅 OnDispatch 事件
            bot.AtMessageAction += async (sender, e, type) =>
            {
                await sender.SendMessageAsync(e.ChannelId, new MessageToCreate()
                {
                    Content = e.Content.TrimStartString(MsgTag.UserTag(bot.UserInfo?.Id), MsgTag.UserTag(e.Author.Id)),
                    MsgId = e.Id
                });
            };

            // 订阅进程退出事件
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                bot.Close();
            };

            // 启动机器人
            bot.Start();

            // 异步程序需要阻塞进程
            while (true) await Task.Delay(1000);
        }
    }
}