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
        /// 频道管理助手BOT用户
        /// <para>用于测试功能接口</para>
        /// </summary>
        private static readonly User TestUser = new() { Id = "977671216794244851" };
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
                Log.Info($"[Benchmarks] {testItem}");
            }
            else
            {
                ++Index;
                testItem = $"{Index:00}｜跳过｜{testItem}";
                Log.Info($"[Benchmarks] {testItem}");
            }
            return testItem;
        }
        /// <summary>
        /// 开始执行基准测试
        /// </summary>
        /// <param name="sender">发起测试的消息对象</param>
        /// <returns></returns>
        public static async Task<int> Run(Sender sender)
        {
            Good = 0;
            Fail = 0;
            Index = 0;
            bool isOk = false;

            List<APIPermission>? guildPermissions = await sender.GetGuildPermissionsAsync();
            if (guildPermissions == null)
            {
                await sender.ReplyAsync("获取频道权限列表失败,请重试...");
                GetLog("获取频道权限列表", false);
                return Index;
            }
            var guild = sender.Bot.Guilds.TryGetValue(sender.GuildId, out var valGuild) ? valGuild : new() { Id = sender.GuildId };
            guild.APIPermissions = guildPermissions;

            string TestName = "发送消息";
            BotAPI api = APIList.发送消息;
            bool hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth)
            {
                Log.Warn($"[Benchmarks] 没有发送消息权限");
                await sender.SendPermissionDemandAsync(new() { Path = APIList.创建频道接口授权链接.Path, Method = APIList.创建频道接口授权链接.Method.Method }, "允许授权后才能正常使用【自检】功能");
                for (int i = 0; i < 10; ++i)
                {
                    await Task.Delay(1000);
                    var sendMsgPermission = await sender.GetGuildPermissionsAsync();
                    if (sendMsgPermission?.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1)) == true)
                    {
                        guildPermissions = sendMsgPermission;
                        Log.Info($"[Benchmarks] 已取得发送消息权限");
                        break;
                    }
                    if (i == 9) return Index;
                }
            }
            Message? message = await sender.ReplyAsync($"自检开始...");
            Message? msgQuote = await sender.ReplyAsync($"功能测试：引用消息", true);
            Message? msgArk = await sender.ReplyAsync(new MsgEmbed($"功能测试：Ark消息发送"));
            TestName = "撤回消息";
            api = APIList.撤回消息;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (hasAuth)
            {
                if (msgQuote != null) isOk |= await sender.DeleteMessageAsync(msgQuote);
                if (msgArk != null) isOk |= await sender.DeleteMessageAsync(msgArk);
                if (!isOk)
                {
                    message = await sender.ReplyAsync($"功能测试：{TestName}");
                    isOk |= message != null && await sender.DeleteMessageAsync(message);
                }
            }
            await sender.ReplyAsync(GetLog("普通消息发送", message != null));
            await sender.ReplyAsync(GetLog("引用消息发送", msgQuote != null));
            await sender.ReplyAsync(GetLog("Ark消息发送", msgArk != null));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
            else await sender.ReplyAsync(GetLog(TestName, isOk));
            TestName = "获取消息列表";
            api = APIList.获取消息列表;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
            else await sender.ReplyAsync(GetLog(TestName, await sender.GetMessagesAsync() != null));
            TestName = "获取指定消息";
            api = APIList.获取指定消息;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
            else await sender.ReplyAsync(GetLog(TestName, await sender.GetMessageAsync(sender.Message) != null));

            TestName = "创建私信会话";
            DirectMessageSource? dms = await sender.CreateDMSAsync(sender.Author);
            await sender.ReplyAsync(GetLog(TestName, dms != null));
            TestName = "发送私信";
            if (dms != null) await sender.ReplyAsync(GetLog(TestName, await sender.SendPMAsync(new MsgText($"功能测试：{TestName}"), dms.Value.GuildId) != null));
            else await sender.ReplyAsync(GetLog($"{TestName}（创建私信会话失败）"));

            await sender.ReplyAsync(GetLog("获取当前用户(机器人)信息", await sender.GetMeAsync() != null));
            await sender.ReplyAsync(GetLog("获取当前用户(机器人)所在频道列表", await sender.GetMeGuildsAsync() != null));

            TestName = "获取频道详情";
            api = APIList.获取频道详情;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
            else await sender.ReplyAsync(GetLog(TestName, await sender.GetGuildAsync() != null));

            TestName = "获取子频道列表";
            api = APIList.获取子频道列表;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
            List<Channel>? channels = hasAuth ? await sender.GetChannelsAsync() : new();
            if (hasAuth) await sender.ReplyAsync(GetLog(TestName, channels != null));
            TestName = "获取子频道详情";
            api = APIList.获取子频道详情;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
            else await sender.ReplyAsync(GetLog(TestName, await sender.GetChannelAsync() != null));
            TestName = "创建子频道";
            api = APIList.创建子频道;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（私域|没有权限）"));
            else
            {
                var channel = await sender.CreateChannelAsync(new() { Name = $"功能测试：{TestName}", Type = ChannelType.文字 });
                await sender.ReplyAsync(GetLog($"{TestName}（私域）", channel != null));
                TestName = "修改子频道";
                api = APIList.修改子频道;
                hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
                if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（私域|没有权限）"));
                else if (channel == null) await sender.ReplyAsync(GetLog($"{TestName}（私域|创建子频道失败）"));
                else
                {
                    channel.Name = $"功能测试：{TestName}";
                    await sender.ReplyAsync(GetLog($"{TestName}（私域）", await sender.EditChannelAsync(channel) != null));
                }
                TestName = "删除子频道";
                api = APIList.删除子频道;
                hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
                if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（私域|没有权限）"));
                else if (channel == null) await sender.ReplyAsync(GetLog($"{TestName}（私域|创建子频道失败）"));
                else await sender.ReplyAsync(GetLog($"{TestName}（私域）", await sender.DeleteChannelAsync(channel)));
            }

            TestName = "获取频道成员列表";
            api = APIList.获取频道成员列表;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（私域|没有权限）"));
            else await sender.ReplyAsync(GetLog($"{TestName}（私域）", await sender.GetGuildMembersAsync() != null));
            TestName = "获取成员详情";
            api = APIList.获取成员详情;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
            else await sender.ReplyAsync(GetLog(TestName, await sender.GetMemberAsync(sender.Bot.Info) != null));
            TestName = "删除频道成员";
            api = APIList.删除频道成员;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（私域|没有权限）"));
            else
            {
                //await sender.ReplyAsync(GetLog("删除频道成员:频道管理助手（私域）", await sender.DeleteGuildMemberAsync(TestUser)));
                await sender.ReplyAsync(GetLog("删除频道成员（私域｜不适合测试）"));    // 此项已单独测试通过
            }

            TestName = "获取频道身份组列表";
            api = APIList.获取频道身份组列表;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
            else await sender.ReplyAsync(GetLog(TestName, await sender.GetGuildMembersAsync() != null));
            TestName = "创建频道身份组";
            api = APIList.创建频道身份组;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
            Role? role = hasAuth ? (await sender.CreateRoleAsync(new Info(TestName, Color.Red))) : null;
            if (hasAuth) await sender.ReplyAsync(GetLog(TestName, role != null));
            if (role == null)
            {
                await sender.ReplyAsync(GetLog("增加频道身份组成员（创建身份组失败）"));
                await sender.ReplyAsync(GetLog("删除频道身份组成员（创建身份组失败）"));
                await sender.ReplyAsync(GetLog("修改频道身份组（创建身份组失败）"));
                await sender.ReplyAsync(GetLog("删除频道身份组（创建身份组失败）"));
            }
            else
            {
                TestName = "添加频道身份组成员";
                api = APIList.添加频道身份组成员;
                hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
                if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
                else await sender.ReplyAsync(GetLog(TestName, await sender.AddRoleMemberAsync(sender.Bot.Info, role.Id)));
                TestName = "删除频道身份组成员";
                api = APIList.删除频道身份组成员;
                hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
                if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
                else await sender.ReplyAsync(GetLog(TestName, await sender.DeleteRoleMemberAsync(sender.Bot.Info, role.Id)));
                TestName = "修改频道身份组";
                api = APIList.修改频道身份组;
                hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
                if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
                else await sender.ReplyAsync(GetLog(TestName, await sender.EditRoleAsync(role.Id, new Info(TestName, Color.Orange)) != null));
                TestName = "删除频道身份组";
                api = APIList.删除频道身份组;
                hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
                if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
                else await sender.ReplyAsync(GetLog(TestName, await sender.DeleteRoleAsync(role.Id)));
            }

            TestName = "获取子频道用户权限";
            api = APIList.获取子频道用户权限;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
            else await sender.ReplyAsync(GetLog(TestName, await sender.GetChannelPermissionsAsync(sender.Bot.Info) != null));
            TestName = "修改子频道用户权限";
            api = APIList.修改子频道用户权限;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
            else await sender.ReplyAsync(GetLog(TestName, await sender.EditChannelPermissionsAsync(PrivacyType.查看 | PrivacyType.发言, TestUser)));
            TestName = "获取子频道身份组权限";
            api = APIList.获取子频道身份组权限;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
            else await sender.ReplyAsync(GetLog(TestName, await sender.GetMemberChannelPermissionsAsync("1") != null));
            TestName = "修改子频道身份组权限";
            api = APIList.修改子频道身份组权限;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
            else await sender.ReplyAsync(GetLog(TestName, await sender.EditMemberChannelPermissionsAsync(PrivacyType.查看 | PrivacyType.发言, "1")));

            TestName = "禁言全员";
            api = APIList.禁言全员;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
            else await sender.ReplyAsync(GetLog(TestName, await sender.MuteGuildAsync(new MuteTime(3))));
            TestName = "禁言指定成员";
            api = APIList.禁言指定成员;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
            else await sender.ReplyAsync(GetLog($"{TestName}(频道管理助手)", await sender.MuteMemberAsync(TestUser, new MuteTime(3))));

            TestName = "创建频道公告";
            api = APIList.创建频道公告;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
            else await sender.ReplyAsync(GetLog(TestName, await sender.CreateAnnouncesGlobalAsync(sender.Message) != null));
            TestName = "删除频道公告";
            api = APIList.删除频道公告;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
            else await sender.ReplyAsync(GetLog(TestName, await sender.DeleteAnnouncesGlobalAsync()));
            TestName = "创建子频道公告";
            api = APIList.创建子频道公告;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
            else await sender.ReplyAsync(GetLog(TestName, await sender.CreateAnnouncesAsync(sender.Message) != null));
            TestName = "删除子频道公告";
            api = APIList.删除子频道公告;
            hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
            if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
            else await sender.ReplyAsync(GetLog(TestName, await sender.DeleteAnnouncesAsync()));

            Channel? schChannel = channels?.Find(c => (c.Type == ChannelType.应用) && (c.ApplicationId == "1000050"));
            if (schChannel == null)
            {
                await sender.ReplyAsync(GetLog("创建日程（获取日程频道失败）"));
                await sender.ReplyAsync(GetLog("获取日程列表（获取日程频道失败）"));
                await sender.ReplyAsync(GetLog("获取日程详情（获取日程频道失败）"));
                await sender.ReplyAsync(GetLog("修改日程（获取日程频道失败）"));
                await sender.ReplyAsync(GetLog("删除日程（获取日程频道失败）"));
            }
            else
            {
                TestName = "创建日程";
                api = APIList.创建日程;
                hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
                if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
                Schedule? schedule = hasAuth ? await sender.CreateScheduleAsync(schChannel.Id, new Schedule(TestName)) : null;
                if (hasAuth) await sender.ReplyAsync(GetLog(TestName, schedule != null));
                TestName = "获取频道日程列表";
                api = APIList.获取频道日程列表;
                hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
                if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
                List<Schedule>? schedules = hasAuth ? await sender.GetSchedulesAsync(schChannel.Id) : null;
                if (hasAuth) await sender.ReplyAsync(GetLog(TestName, schedules != null));
                TestName = "获取日程详情";
                api = APIList.获取日程详情;
                hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
                if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
                else if ((schedule != null) || (schedules?.Any() == true))
                {
                    string scheduleId = (schedule?.Id ?? schedules?.First().Id)!;
                    await sender.ReplyAsync(GetLog(TestName, await sender.GetScheduleAsync(schChannel.Id, scheduleId) != null));
                }
                else await sender.ReplyAsync(GetLog($"{TestName}（找不到日程）"));
                if (schedule == null)
                {
                    await sender.ReplyAsync(GetLog("修改日程（创建日程失败）"));
                    await sender.ReplyAsync(GetLog("删除日程（创建日程失败）"));
                }
                else
                {
                    TestName = "修改日程";
                    api = APIList.修改日程;
                    hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
                    if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
                    else
                    {
                        schedule.Name = TestName;
                        schedule.StartTime = DateTime.Now.AddHours(1);
                        schedule.EndTime = schedule.StartTime.AddDays(1);
                        await sender.ReplyAsync(GetLog(TestName, await sender.EditScheduleAsync(schChannel.Id, schedule) != null));
                    }
                    TestName = "删除日程";
                    api = APIList.删除日程;
                    hasAuth = guildPermissions.Any(p => (api.Path == p.Path) && (api.Method.Method.ToUpper() == p.Method) && (p.AuthStatus == 1));
                    if (!hasAuth) await sender.ReplyAsync(GetLog($"{TestName}（没有权限）"));
                    else await sender.ReplyAsync(GetLog(TestName, await sender.DeleteScheduleAsync(schChannel.Id, schedule)));
                }
            }

            /*这里需要添加音频API测试代码*/
            await sender.ReplyAsync(GetLog("音频API（暂时无法测试）"));

            string completedMsg = $"自检完成：共{Index}项｜通过{Good}项｜失败{Fail}项｜跳过{Index - Good - Fail}项";
            await sender.ReplyAsync(completedMsg);
            Log.Info($"[Benchmarks]{completedMsg}");
            return Index;
        }
    }
}
