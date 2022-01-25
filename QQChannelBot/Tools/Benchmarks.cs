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
        public static async Task<int> Run(Sender sender)
        {
            Good = 0;
            Fail = 0;
            Index = 0;
            bool isOk;

            Message? message = await sender.ReplyAsync("自检开始...\n01｜通过｜消息发送");
            GetLog("消息发送", message != null);
            if (message == null) return Index;

            await sender.ReplyAsync(GetLog("获取消息列表", await sender.GetMessagesAsync() != null));

            message = await sender.GetMessageAsync(sender.Message);
            await sender.ReplyAsync(GetLog("获取指定消息", message != null));

            message = await sender.ReplyAsync(new MsgEmbed("功能测试：Ark消息发送"));
            await sender.ReplyAsync(GetLog("Ark消息发送", message != null));

            if (message == null)
            {
                message = await sender.ReplyAsync($"04｜失败｜撤回消息");
                isOk = message != null && await sender.DeleteMessageAsync(message);
                if (isOk) await sender.ReplyAsync(GetLog("撤回消息", isOk));
                else IncFail();
            }
            else
            {
                isOk = await sender.DeleteMessageAsync(message);
                await sender.ReplyAsync(GetLog("撤回消息", isOk));
            }

            var guid = await sender.GetGuildAsync();
            await sender.ReplyAsync(GetLog("获取频道详情", guid != null));

            List<Role>? roles = await sender.GetRolesAsync();
            await sender.ReplyAsync(GetLog("获取频道身份组列表", roles != null));

            Role? role = await sender.CreateRoleAsync(new Info("创建频道身份组", Color.Red));
            await sender.ReplyAsync(GetLog("创建频道身份组", role != null));

            if (role != null)
            {
                await sender.ReplyAsync(GetLog("增加频道身份组成员", await sender.AddRoleMemberAsync(sender.Bot.Info, role.Id)));
                await sender.ReplyAsync(GetLog("删除频道身份组成员", await sender.DeleteRoleMemberAsync(sender.Bot.Info, role.Id)));
                await sender.ReplyAsync(GetLog("修改频道身份组", await sender.EditRoleAsync(role.Id, new Info("修改频道身份组", Color.Orange)) != null));
                await sender.ReplyAsync(GetLog("删除频道身份组", await sender.DeleteRoleAsync(role.Id)));
            }
            else
            {
                await sender.ReplyAsync(GetLog("增加频道身份组成员（创建身份失败）"));
                await sender.ReplyAsync(GetLog("删除频道身份组成员"));
                await sender.ReplyAsync(GetLog("修改频道身份组"));
                await sender.ReplyAsync(GetLog("删除频道身份组"));
            }

            Member? member = await sender.GetMemberAsync(sender.Bot.Info);
            await sender.ReplyAsync(GetLog("获取频道成员详情", member != null));

            List<Member>? memberList = await sender.GetGuildMembersAsync();
            await sender.ReplyAsync(GetLog("获取频道成员列表（私域）", memberList != null));

            //isOk = await sender.DeleteGuildMemberAsync(TestUser);
            //await sender.ReplyAsync(GetLog("删除频道成员:频道管理助手（私域）", isOk));
            await sender.ReplyAsync(GetLog("删除频道成员（私域｜不适合测试）"));    // 此项已单独测试通过

            Announces? announces = await sender.CreateAnnouncesGlobalAsync(sender.Message);
            await sender.ReplyAsync(GetLog("创建频道全局公告", announces != null));
            if (announces != null)
            {
                await sender.ReplyAsync(GetLog("删除频道全局公告", await sender.DeleteAnnouncesGlobalAsync()));
            }
            else await sender.ReplyAsync(GetLog("删除频道全局公告（创建公告失败）"));

            announces = await sender.CreateAnnouncesAsync(sender.Message);
            await sender.ReplyAsync(GetLog("创建子频道公告", announces != null));
            if (announces != null)
            {
                await sender.ReplyAsync(GetLog("删除子频道公告", await sender.DeleteAnnouncesAsync()));
            }
            else await sender.ReplyAsync(GetLog("删除子频道公告（创建公告失败）"));

            List<Channel>? channels = await sender.GetChannelsAsync();
            await sender.ReplyAsync(GetLog("获取子频道列表", channels != null));
            Channel? channel = await sender.GetChannelAsync();
            await sender.ReplyAsync(GetLog("获取子频道详情", channel != null));

            if (channel != null)
            {
                channel = await sender.CreateChannelAsync(new Channel() { Name = "Benchmaarks测试创建频道", Type = ChannelType.文字 });
                await sender.ReplyAsync(GetLog("创建子频道（私域）", channel != null));
            }
            else await sender.ReplyAsync(GetLog("创建子频道（私域｜获取频道分组失败）"));
            if (channel?.Name == "Benchmaarks测试创建频道")
            {
                channel.Name = "Benchmaarks测试修改频道";
                await sender.ReplyAsync(GetLog("修改子频道（私域）", await sender.EditChannelAsync(channel) != null));
                await sender.ReplyAsync(GetLog("删除子频道（私域）", await sender.DeleteChannelAsync(channel)));
            }
            else
            {
                await sender.ReplyAsync(GetLog("修改子频道（私域｜创建子频道失败）"));
                await sender.ReplyAsync(GetLog("删除子频道（私域｜创建子频道失败）"));
            }

            ChannelPermissions? permissions = await sender.GetChannelPermissionsAsync(TestUser); // 获取"频道管理助手"的权限
            await sender.ReplyAsync(GetLog("获取用户在指定(当前)子频道的权限", permissions != null));

            isOk = await sender.EditChannelPermissionsAsync(permissions?.Permissions ?? PrivacyType.查看, TestUser); // 修改"频道管理助手"的权限
            await sender.ReplyAsync(GetLog("修改用户在指定(当前)子频道的权限", isOk));

            permissions = await sender.GetMemberChannelPermissionsAsync("1");   // 获取"1"普通成员的权限
            await sender.ReplyAsync(GetLog("获取指定身份组在指定(当前)子频道的权限", permissions != null));

            isOk = await sender.EditMemberChannelPermissionsAsync(permissions?.Permissions ?? PrivacyType.查看, "1");     // 修改"1"普通成员的权限
            await sender.ReplyAsync(GetLog("修改指定身份组在指定(当前)子频道的权限", isOk));

            /*这里需要添加音频API测试代码*/
            await sender.ReplyAsync(GetLog("音频API（暂时无法测试）"));

            User? user = await sender.GetMeAsync();
            await sender.ReplyAsync(GetLog("获取当前用户(机器人)信息", user != null));

            List<Guild>? guilds = await sender.GetMeGuildsAsync();
            await sender.ReplyAsync(GetLog("获取当前用户(机器人)所在频道列表", guilds != null));

            Channel? schChannel = channels == null ? null : schChannel = channels.Find(c => c.Name!.Contains("日程"));
            if (schChannel != null)
            {
                Schedule? schedule = await sender.CreateScheduleAsync(schChannel.Id, new Schedule("测试创建日程"));
                await sender.ReplyAsync(GetLog("创建日程", schedule != null));
                await sender.ReplyAsync(GetLog("获取日程列表", await sender.GetSchedulesAsync(schChannel.Id) != null));
                if (schedule != null)
                {
                    await sender.ReplyAsync(GetLog("获取单个日程", (await sender.GetScheduleAsync(schChannel.Id, schedule.Id!)) != null));
                    schedule.Name = "测试修改日程";
                    await sender.ReplyAsync(GetLog("修改日程", await sender.EditScheduleAsync(schChannel.Id, schedule) != null));
                    await sender.ReplyAsync(GetLog("删除日程", await sender.DeleteScheduleAsync(schChannel.Id, schedule)));
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

            isOk = await sender.MuteGuildAsync(new MuteTime(3));
            await sender.ReplyAsync(GetLog("频道全局禁言", isOk));

            isOk = await sender.MuteMemberAsync(TestUser, new MuteTime(3)); // 禁言"频道管理助手"用于测试
            await sender.ReplyAsync(GetLog("频道指定成员(频道管理助手)禁言", isOk));

            string completedMsg = $"自检完成：共{Index}项｜通过{Good}项｜失败{Fail}项｜跳过{Index - Good - Fail}项";
            await sender.ReplyAsync(completedMsg);
            Log.Info($"[Benchmarks]{completedMsg}");
            return Index;
        }
    }
}
