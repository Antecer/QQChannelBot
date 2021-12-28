using QQChannelBot.Bot;
using QQChannelBot.Models;
using QQChannelBot.MsgHelper;

namespace QQChannelBot.Tools
{
    public static class Benchmarks
    {
        public static int Good { get; set; }
        public static int Fail { get; set; }
        private static int Index { get; set; }
        private static int IncGood() { ++Index; return ++Good; }
        private static int IncFail() { ++Index; return ++Fail; }
        private static string GetLog(string testItem, bool? isOk = null)
        {
            if (isOk != null)
            {
                _ = isOk.Value ? IncGood() : IncFail();
                testItem = (isOk.Value ? "【通过】" : "【失败】") + testItem;
                Log.Info($"[Benchmarks][{Index}]{testItem}");
            }
            else
            {
                ++Index;
                testItem = "【跳过】" + testItem;
                Log.Info($"[Benchmarks][{Index}]{testItem}");
            }
            return testItem;
        }
        public static async Task<int> Run(Message sender)
        {
            Good = 0;
            Fail = 0;
            Index = 0;
            bool isOk;
            BotClient bot = sender.Bot!;
            string botId = bot.Info!.Id;
            bot.ReportApiError = false;

            Message? message = await sender.ReplyAsync("自检开始...\n【通过】消息发送");
            GetLog("消息发送", message != null);
            if (message == null)
            {
                bot.ReportApiError = true;
                return Index;
            }

            message = await bot.GetMessageAsync(message.ChannelId, message.Id);
            await sender.ReplyAsync(GetLog("获取指定消息", message != null));

            message = await sender.ReplyAsync(new MsgEmbed("功能测试：Ark消息发送"));
            await sender.ReplyAsync(GetLog("Ark消息发送", message != null));

            if (message == null)
            {
                message = await sender.ReplyAsync($"【失败】撤回消息");
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

            Role? role = await bot.CreateRoleAsync(sender.GuildId, new Info("创建频道身份组", "#F00"));
            await sender.ReplyAsync(GetLog("创建频道身份组", role != null));

            if (role != null)
            {
                isOk = await bot.AddRoleMemberAsync(sender.GuildId, botId, role.Id);
                await sender.ReplyAsync(GetLog("增加频道身份组成员", isOk));
            }
            else await sender.ReplyAsync(GetLog("增加频道身份组成员"));

            if (role != null)
            {
                isOk = await bot.DeleteRoleMemberAsync(sender.GuildId, botId, role.Id);
                await sender.ReplyAsync(GetLog("删除频道身份组成员", isOk));
            }
            else await sender.ReplyAsync(GetLog("删除频道身份组成员"));

            if (role != null)
            {
                role = await bot.EditRoleAsync(sender.GuildId, role.Id, new Info("修改频道身份组", "#FF0"));
                await sender.ReplyAsync(GetLog("修改频道身份组", role != null));
            }
            else await sender.ReplyAsync(GetLog("修改频道身份组"));

            if (role != null)
            {
                isOk = await bot.DeleteRoleAsync(sender.GuildId, role.Id);
                await sender.ReplyAsync(GetLog("删除频道身份组", isOk));
            }
            else await sender.ReplyAsync(GetLog("删除频道身份组"));

            Member? member = await bot.GetMemberAsync(sender.GuildId, botId);
            await sender.ReplyAsync(GetLog("获取用户成员信息", member != null));

            Announces? announces = await bot.CreateAnnouncesGlobalAsync(sender.GuildId, sender.ChannelId, sender.Id);
            await sender.ReplyAsync(GetLog("创建频道全局公告", announces != null));

            if (announces != null)
            {
                isOk = await bot.DeleteAnnouncesGlobalAsync(sender.GuildId, sender.Id);
                await sender.ReplyAsync(GetLog("删除频道全局公告", isOk));
            }
            else await sender.ReplyAsync(GetLog("删除频道全局公告"));

            announces = await bot.CreateAnnouncesAsync(sender.ChannelId, sender.Id);
            await sender.ReplyAsync(GetLog("创建子频道公告", announces != null));

            if (announces != null)
            {
                isOk = await bot.DeleteAnnouncesAsync(sender.ChannelId);
                await sender.ReplyAsync(GetLog("创建子频道公告", isOk));
            }
            else await sender.ReplyAsync(GetLog("创建子频道公告"));

            Channel? channel = await bot.GetChannelAsync(sender.ChannelId);
            await sender.ReplyAsync(GetLog("获取子频道信息", channel != null));

            List<Channel>? channels = await bot.GetChannelsAsync(sender.GuildId);
            await sender.ReplyAsync(GetLog("获取频道下的子频道列表", channels != null));

            ChannelPermissions? permissions = await bot.GetChannelPermissionsAsync(sender.ChannelId, botId);
            await sender.ReplyAsync(GetLog("获取用户在指定(当前)子频道的权限", permissions != null));

            isOk = await bot.EditChannelPermissionsAsync(sender.ChannelId, botId, "1");
            await sender.ReplyAsync(GetLog("修改用户在指定(当前)子频道的权限", isOk));

            permissions = await bot.GetMemberChannelPermissionsAsync(sender.ChannelId, "1");
            await sender.ReplyAsync(GetLog("获取指定身份组在指定(当前)子频道的权限", permissions != null));

            isOk = await bot.EditMemberChannelPermissionsAsync(sender.ChannelId, botId, "1");
            await sender.ReplyAsync(GetLog("修改指定身份组在指定(当前)子频道的权限", isOk));

            /*这里需要添加音频API测试代码*/
            await sender.ReplyAsync(GetLog("音频API"));

            User? user = await bot.GetMeAsync();
            await sender.ReplyAsync(GetLog("获取当前用户(机器人)信息", user != null));

            List<Guild>? guilds = await bot.GetMeGuildsAsync();
            await sender.ReplyAsync(GetLog("获取当前用户(机器人)所在频道列表", guilds != null));

            /*这里需要添加音频API测试代码*/
            await sender.ReplyAsync(GetLog("日程API"));

            isOk = await bot.MuteGuildAsync(sender.GuildId, new MuteTime(3));
            await sender.ReplyAsync(GetLog("频道全局禁言", isOk));

            isOk = await bot.MuteMemberAsync(sender.GuildId, "977671216794244851", new MuteTime(3)); // 禁言"频道管理助手"用于测试
            await sender.ReplyAsync(GetLog("频道指定成员(频道管理助手)禁言", isOk));

            await sender.ReplyAsync($"自检完成，共{Index}项|通过{Good}项|失败{Fail}项|跳过{Index - Good - Fail}项。");
            bot.ReportApiError = true;
            return Index;
        }
    }
}
