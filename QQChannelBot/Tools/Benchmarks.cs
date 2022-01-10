using System.Drawing;
using QQChannelBot.Bot;
using QQChannelBot.Models;
using QQChannelBot.MsgHelper;

namespace QQChannelBot.Tools
{
    /// <summary>
    /// API基准测试
    /// </summary>
    public static class Benchmarks
    {
        /// <summary>
        /// 频道管理助手BOT用户Id
        /// <para>用于测试功能接口</para>
        /// </summary>
        private const string TestUserId = "977671216794244851";
        /// <summary>
        /// 测试项成功计数
        /// </summary>
        public static int Good { get; set; }
        /// <summary>
        /// 测试项失败计数
        /// </summary>
        public static int Fail { get; set; }
        /// <summary>
        /// 测试项目计数统计
        /// </summary>
        private static int Index { get; set; }
        /// <summary>
        /// 项目测试成功计数加一
        /// </summary>
        /// <returns></returns>
        private static int IncGood() { ++Index; return ++Good; }
        /// <summary>
        /// 项目测试失败计数加一
        /// </summary>
        /// <returns></returns>
        private static int IncFail() { ++Index; return ++Fail; }
        /// <summary>
        /// 根据测试项结果组装Log信息
        /// </summary>
        /// <param name="testItem">测试项名称</param>
        /// <param name="isOk">测试项结果</param>
        /// <returns></returns>
        private static string GetLog(string testItem, bool? isOk = null)
        {
            if (isOk != null)
            {
                _ = isOk.Value ? IncGood() : IncFail();
                testItem = $"{Index:00}{(isOk.Value ? "｜通过｜" : "｜失败｜")}{testItem}";
                Log.Info($"[Benchmarks]{testItem}");
            }
            else
            {
                ++Index;
                testItem = $"{Index:00}｜跳过｜{testItem}";
                Log.Info($"[Benchmarks]{testItem}");
            }
            return testItem;
        }
        /// <summary>
        /// 开始执行基准测试
        /// </summary>
        /// <param name="sender">发起测试的消息对象</param>
        /// <returns></returns>
        public static async Task<int> Run(Message sender)
        {
            Good = 0;
            Fail = 0;
            Index = 0;
            bool isOk;
            BotClient bot = sender.Bot!;
            string botId = bot.Info.Id;
            bool? tmpReportApiError = bot.ReportApiError;
            bot.ReportApiError = false;

            Message? message = await sender.ReplyAsync("自检开始...\n01｜通过｜消息发送");
            GetLog("消息发送", message != null);
            if (message == null)
            {
                bot.ReportApiError = tmpReportApiError;
                return Index;
            }

            message = await bot.GetMessageAsync(message.ChannelId, message.Id);
            await sender.ReplyAsync(GetLog("获取指定消息", message != null));

            message = await sender.ReplyAsync(new MsgEmbed("功能测试：Ark消息发送"));
            await sender.ReplyAsync(GetLog("Ark消息发送", message != null));

            if (message == null)
            {
                message = await sender.ReplyAsync($"04｜失败｜撤回消息");
                isOk = message != null && await bot.DeleteMessageAsync(message.ChannelId, message.Id);
                if (isOk) await sender.ReplyAsync(GetLog("撤回消息", isOk));
                else IncFail();
            }
            else
            {
                isOk = await bot.DeleteMessageAsync(message.ChannelId, message.Id);
                await sender.ReplyAsync(GetLog("撤回消息", isOk));
            }

            var guid = await bot.GetGuildAsync(sender.GuildId);
            await sender.ReplyAsync(GetLog("获取频道详情", guid != null));

            List<Role>? roles = await bot.GetRolesAsync(sender.GuildId);
            await sender.ReplyAsync(GetLog("获取频道身份组列表", roles != null));

            Role? role = await bot.CreateRoleAsync(sender.GuildId, new Info("创建频道身份组", Color.Red));
            await sender.ReplyAsync(GetLog("创建频道身份组", role != null));

            if (role != null)
            {
                await sender.ReplyAsync(GetLog("增加频道身份组成员", await bot.AddRoleMemberAsync(sender.GuildId, botId, role.Id)));
                await sender.ReplyAsync(GetLog("删除频道身份组成员", await bot.DeleteRoleMemberAsync(sender.GuildId, botId, role.Id)));
                await sender.ReplyAsync(GetLog("修改频道身份组", await bot.EditRoleAsync(sender.GuildId, role.Id, new Info("修改频道身份组", Color.Orange)) != null));
                await sender.ReplyAsync(GetLog("删除频道身份组", await bot.DeleteRoleAsync(sender.GuildId, role.Id)));
            }
            else
            {
                await sender.ReplyAsync(GetLog("增加频道身份组成员（创建身份失败）"));
                await sender.ReplyAsync(GetLog("删除频道身份组成员"));
                await sender.ReplyAsync(GetLog("修改频道身份组"));
                await sender.ReplyAsync(GetLog("删除频道身份组"));
            }

            Member? member = await bot.GetMemberAsync(sender.GuildId, botId);
            await sender.ReplyAsync(GetLog("获取频道成员详情", member != null));

            List<Member>? memberList = await bot.GetGuildMembersAsync(sender.GuildId);
            await sender.ReplyAsync(GetLog("获取频道成员列表（私域）", memberList != null));

            //isOk = await bot.DeleteGuildMemberAsync(sender.GuildId, TestUserId);
            //await sender.ReplyAsync(GetLog("删除频道成员:频道管理助手（私域）", isOk));
            await sender.ReplyAsync(GetLog("删除频道成员（私域｜不适合测试）"));

            Announces? announces = await bot.CreateAnnouncesGlobalAsync(sender.GuildId, sender.ChannelId, sender.Id);
            await sender.ReplyAsync(GetLog("创建频道全局公告", announces != null));
            if (announces != null)
            {
                await sender.ReplyAsync(GetLog("删除频道全局公告", await bot.DeleteAnnouncesGlobalAsync(sender.GuildId)));
            }
            else await sender.ReplyAsync(GetLog("删除频道全局公告（创建公告失败）"));

            announces = await bot.CreateAnnouncesAsync(sender.ChannelId, sender.Id);
            await sender.ReplyAsync(GetLog("创建子频道公告", announces != null));
            if (announces != null)
            {
                await sender.ReplyAsync(GetLog("删除子频道公告", await bot.DeleteAnnouncesAsync(sender.ChannelId)));
            }
            else await sender.ReplyAsync(GetLog("删除子频道公告（创建公告失败）"));

            List<Channel>? channels = await bot.GetChannelsAsync(sender.GuildId);
            await sender.ReplyAsync(GetLog("获取频道下的子频道列表", channels != null));
            Channel? channel = await bot.GetChannelAsync(sender.ChannelId);
            await sender.ReplyAsync(GetLog("获取子频道信息", channel != null));

            if (channel != null)
            {
                channel = await bot.CreateChannelAsync(sender.GuildId, "Benchmaarks测试创建频道", ChannelType.文字, 1, channel.ParentId!);
                await sender.ReplyAsync(GetLog("创建子频道（私域）", channel != null));
            }
            else await sender.ReplyAsync(GetLog("创建子频道（私域｜获取频道分组失败）"));
            if (channel?.Name == "Benchmaarks测试创建频道")
            {
                await sender.ReplyAsync(GetLog("修改子频道（私域）", await bot.EditChannelAsync(channel.Id!, "Benchmaarks测试修改频道", ChannelType.文字, 1, channel.ParentId!) != null));
                await sender.ReplyAsync(GetLog("删除子频道（私域）", await bot.DeleteChannelAsync(channel.Id!)));
            }
            else
            {
                await sender.ReplyAsync(GetLog("修改子频道（私域｜创建子频道失败）"));
                await sender.ReplyAsync(GetLog("删除子频道（私域｜创建子频道失败）"));
            }

            ChannelPermissions? permissions = await bot.GetChannelPermissionsAsync(sender.ChannelId, TestUserId); // 获取"频道管理助手"的权限
            await sender.ReplyAsync(GetLog("获取用户在指定(当前)子频道的权限", permissions != null));

            isOk = await bot.EditChannelPermissionsAsync(sender.ChannelId, TestUserId, "1"); // 修改"频道管理助手"的权限
            await sender.ReplyAsync(GetLog("修改用户在指定(当前)子频道的权限", isOk));

            permissions = await bot.GetMemberChannelPermissionsAsync(sender.ChannelId, "1");    // 获取"1"普通成员的权限
            await sender.ReplyAsync(GetLog("获取指定身份组在指定(当前)子频道的权限", permissions != null));

            isOk = await bot.EditMemberChannelPermissionsAsync(sender.ChannelId, "1", "1");     // 修改"1"普通成员的权限
            await sender.ReplyAsync(GetLog("修改指定身份组在指定(当前)子频道的权限", isOk));

            /*这里需要添加音频API测试代码*/
            await sender.ReplyAsync(GetLog("音频API（暂时无法测试）"));

            User? user = await bot.GetMeAsync();
            await sender.ReplyAsync(GetLog("获取当前用户(机器人)信息", user != null));

            List<Guild>? guilds = await bot.GetMeGuildsAsync();
            await sender.ReplyAsync(GetLog("获取当前用户(机器人)所在频道列表", guilds != null));

            Channel? schChannel = channels == null ? null : schChannel = channels.Find(c => c.Name!.Contains("日程"));
            if (schChannel != null)
            {
                Schedule? schedule = await bot.CreateScheduleAsync(schChannel.Id, new Schedule("测试创建日程"));
                await sender.ReplyAsync(GetLog("创建日程", schedule != null));
                await sender.ReplyAsync(GetLog("获取日程列表", await bot.GetSchedulesAsync(schChannel.Id) != null));
                if (schedule != null)
                {
                    await sender.ReplyAsync(GetLog("获取单个日程", (await bot.GetScheduleAsync(schChannel.Id, schedule.Id!)) != null));
                    schedule.Name = "测试修改日程";
                    await sender.ReplyAsync(GetLog("修改日程", await bot.EditScheduleAsync(schChannel.Id, schedule) != null));
                    await sender.ReplyAsync(GetLog("删除日程", await bot.DeleteScheduleAsync(schChannel.Id, schedule)));
                }
                else
                {
                    await sender.ReplyAsync(GetLog("获取单个日程（创建日程失败）"));
                    await sender.ReplyAsync(GetLog("修改日程（创建日程失败）"));
                    await sender.ReplyAsync(GetLog("删除日程（创建日程失败）"));
                }
            }
            else
            {
                await sender.ReplyAsync(GetLog("创建日程（获取日程频道失败）"));
                await sender.ReplyAsync(GetLog("获取单个日程（获取日程频道失败）"));
                await sender.ReplyAsync(GetLog("修改日程（获取日程频道失败）"));
                await sender.ReplyAsync(GetLog("获取日程列表（获取日程频道失败）"));
                await sender.ReplyAsync(GetLog("删除日程（获取日程频道失败）"));
            }

            isOk = await bot.MuteGuildAsync(sender.GuildId, new MuteTime(3));
            await sender.ReplyAsync(GetLog("频道全局禁言", isOk));

            isOk = await bot.MuteMemberAsync(sender.GuildId, TestUserId, new MuteTime(3)); // 禁言"频道管理助手"用于测试
            await sender.ReplyAsync(GetLog("频道指定成员(频道管理助手)禁言", isOk));

            string completedMsg = $"自检完成：共{Index}项｜通过{Good}项｜失败{Fail}项｜跳过{Index - Good - Fail}项";
            await sender.ReplyAsync(completedMsg);
            Log.Info($"[Benchmarks]{completedMsg}");

            bot.ReportApiError = tmpReportApiError;
            return Index;
        }
    }
}
