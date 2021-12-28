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
        private const int Count = 100;
        private static int IncGood() { ++Index; return ++Good; }
        private static int IncFail() { ++Index; return ++Fail; }
        public static async Task<int> Run(Message sender)
        {
            Good = 0;
            Fail = 0;
            Index = 0;
            BotClient bot = sender.Bot!;
            string TestItem = "自检开始...\n【通过】消息发送";
            Message? message = await sender.ReplyAsync(TestItem);
            _ = message != null ? IncGood() : IncFail();
            TestItem = (message != null ? "【通过】" : "【失败】") + "消息发送";
            Log.Info($"[Benchmarks]{TestItem}");
            if (message == null)
            {
                Log.Info($"[Benchmarks]测试中止！");
                IncFail();
                return Index;
            }
            IncGood();

            message = await bot.GetMessageAsync(message.ChannelId, message.Id);
            _ = message != null ? IncGood() : IncFail();
            TestItem = (message != null ? "【通过】" : "【失败】") + "获取指定消息";
            Log.Info($"[Benchmarks]{TestItem}");
            await sender.ReplyAsync(TestItem);

            message = await sender.ReplyAsync(new MsgEmbed("功能测试：Ark消息"));
            _ = message != null ? IncGood() : IncFail();
            TestItem = (message != null ? "【通过】" : "【失败】") + "Ark消息发送";
            Log.Info($"[Benchmarks]{TestItem}");
            await sender.ReplyAsync(TestItem);

            message ??= await sender.ReplyAsync($"功能测试：撤回消息");
            bool isOk = message != null && await bot.DeleteMessageAsync(message.ChannelId, message.Id);
            _ = isOk ? IncGood() : IncFail();
            TestItem = (isOk ? "【通过】" : "【失败】") + "撤回消息";
            Log.Info($"[Benchmarks]{TestItem}");
            await sender.ReplyAsync(TestItem);

            isOk = await bot.GetRolesAsync(sender.GuildId) != null;
            _ = isOk ? IncGood() : IncFail();
            TestItem = (isOk ? "【通过】" : "【失败】") + "获取频道身份组列表";
            Log.Info($"[Benchmarks]{TestItem}");
            await sender.ReplyAsync(TestItem);

            Role? role = await bot.CreateRoleAsync(sender.GuildId, new Info("新增身份组", "#F00"));
            _ = role != null ? IncGood() : IncFail();
            TestItem = (role != null ? "【通过】" : "【失败】") + "创建频道身份组";
            Log.Info($"[Benchmarks]{TestItem}");
            await sender.ReplyAsync(TestItem);

            if (role != null)
            {
                isOk = await bot.AddRoleMemberAsync(sender.GuildId, sender.Bot!.Info!.Id, role.Id);
                _ = isOk ? IncGood() : IncFail();
                TestItem = (isOk ? "【通过】" : "【失败】") + "增加频道身份组成员";
                Log.Info($"[Benchmarks]{TestItem}");
                await sender.ReplyAsync(TestItem);
            }
            else
            {
                ++Index;
                TestItem = "【跳过】增加频道身份组成员";
                Log.Info($"[Benchmarks]{TestItem}");
                await sender.ReplyAsync(TestItem);
            }

            if (role != null)
            {
                isOk = await bot.DeleteRoleMemberAsync(sender.GuildId, sender.Bot!.Info!.Id, role.Id);
                _ = isOk ? IncGood() : IncFail();
                TestItem = (isOk ? "【通过】" : "【失败】") + "删除频道身份组成员";
                Log.Info($"[Benchmarks]{TestItem}");
                await sender.ReplyAsync(TestItem);
            }
            else
            {
                ++Index;
                TestItem = "【跳过】增加频道身份组成员";
                Log.Info($"[Benchmarks]{TestItem}");
                await sender.ReplyAsync(TestItem);
            }

            if (role != null)
            {
                role = await bot.EditRoleAsync(sender.GuildId, role.Id, new Info("修改身份组", "#FF0"));
                _ = role != null ? IncGood() : IncFail();
                TestItem = (role != null ? "【通过】" : "【失败】") + "修改频道身份组";
                Log.Info($"[Benchmarks]{TestItem}");
                await sender.ReplyAsync(TestItem);
            }
            else
            {
                ++Index;
                TestItem = "【跳过】修改频道身份组";
                Log.Info($"[Benchmarks]{TestItem}");
                await sender.ReplyAsync(TestItem);
            }

            if (role != null)
            {
                isOk = await bot.DeleteRoleAsync(sender.GuildId, role.Id);
                _ = isOk ? IncGood() : IncFail();
                TestItem = (isOk ? "【通过】" : "【失败】") + "删除频道身份组";
                Log.Info($"[Benchmarks]{TestItem}");
                await sender.ReplyAsync(TestItem);
            }
            else
            {
                ++Index;
                TestItem = "【跳过】删除频道身份组";
                Log.Info($"[Benchmarks]{TestItem}");
                await sender.ReplyAsync(TestItem);
            }

            Member? member = await bot.GetMemberAsync(sender.GuildId, sender.Bot!.Info!.Id);
            _ = member != null ? IncGood() : IncFail();
            TestItem = (member != null ? "【通过】" : "【失败】") + "获取用户成员信息";
            Log.Info($"[Benchmarks]{TestItem}");
            await sender.ReplyAsync(TestItem);

            Announces? announces = await bot.CreateAnnouncesGlobalAsync(sender.GuildId, sender.ChannelId, sender.Id);
            _ = announces != null ? IncGood() : IncFail();
            TestItem = (announces != null ? "【通过】" : "【失败】") + "创建频道全局公告";
            Log.Info($"[Benchmarks]{TestItem}");
            await sender.ReplyAsync(TestItem);

            if (announces != null)
            {
                isOk = await bot.DeleteAnnouncesGlobalAsync(sender.GuildId, sender.Id);
                _ = isOk ? IncGood() : IncFail();
                TestItem = (isOk ? "【通过】" : "【失败】") + "删除频道全局公告";
                Log.Info($"[Benchmarks]{TestItem}");
                await sender.ReplyAsync(TestItem);
            }
            else
            {
                ++Index;
                TestItem = "【跳过】删除频道全局公告";
                Log.Info($"[Benchmarks]{TestItem}");
                await sender.ReplyAsync(TestItem);
            }

            announces = await bot.CreateAnnouncesAsync(sender.ChannelId, sender.Id);
            _ = announces != null ? IncGood() : IncFail();
            TestItem = (announces != null ? "【通过】" : "【失败】") + "创建子频道公告";
            Log.Info($"[Benchmarks]{TestItem}");
            await sender.ReplyAsync(TestItem);

            if (announces != null)
            {
                isOk = await bot.DeleteAnnouncesAsync(sender.ChannelId);
                _ = isOk ? IncGood() : IncFail();
                TestItem = (isOk ? "【通过】" : "【失败】") + "删除子频道公告";
                Log.Info($"[Benchmarks]{TestItem}");
                await sender.ReplyAsync(TestItem);
            }
            else
            {
                ++Index;
                TestItem = "【跳过】删除子频道公告";
                Log.Info($"[Benchmarks]{TestItem}");
                await sender.ReplyAsync(TestItem);
            }

            Channel? channel = await bot.GetChannelAsync(sender.ChannelId);
            _ = channel != null ? IncGood() : IncFail();
            TestItem = (channel != null ? "【通过】" : "【失败】") + "获取子频道信息";
            Log.Info($"[Benchmarks]{TestItem}");
            await sender.ReplyAsync(TestItem);

            List<Channel>? channels = await bot.GetChannelsAsync(sender.GuildId);
            _ = channels != null ? IncGood() : IncFail();
            TestItem = (channels != null ? "【通过】" : "【失败】") + "获取频道下的子频道列表";
            Log.Info($"[Benchmarks]{TestItem}");
            await sender.ReplyAsync(TestItem);

            ChannelPermissions? permissions = await bot.GetChannelPermissionsAsync(sender.ChannelId, sender.Bot.Info.Id);
            _ = permissions != null ? IncGood() : IncFail();
            TestItem = (permissions != null ? "【通过】" : "【失败】") + "获取用户在指定(当前)子频道的权限";
            Log.Info($"[Benchmarks]{TestItem}");
            await sender.ReplyAsync(TestItem);

            isOk = await bot.EditChannelPermissionsAsync(sender.ChannelId, sender.Bot.Info.Id, "1");
            _ = isOk ? IncGood() : IncFail();
            TestItem = (isOk ? "【通过】" : "【失败】") + "修改用户在指定(当前)子频道的权限";
            Log.Info($"[Benchmarks]{TestItem}");
            await sender.ReplyAsync(TestItem);

            permissions = await bot.GetMemberChannelPermissionsAsync(sender.ChannelId, "1");
            _ = permissions != null ? IncGood() : IncFail();
            TestItem = (permissions != null ? "【通过】" : "【失败】") + "获取指定身份组在指定(当前)子频道的权限";
            Log.Info($"[Benchmarks]{TestItem}");
            await sender.ReplyAsync(TestItem);

            isOk = await bot.EditMemberChannelPermissionsAsync(sender.ChannelId, sender.Bot.Info.Id, "1");
            _ = isOk ? IncGood() : IncFail();
            TestItem = (isOk ? "【通过】" : "【失败】") + "修改指定身份组在指定(当前)子频道的权限";
            Log.Info($"[Benchmarks]{TestItem}");
            await sender.ReplyAsync(TestItem);

            await sender.ReplyAsync($"自检完成，共{Index}项|通过{Good}项|失败{Fail}项|跳过{Index - Good - Fail}。");
            return Index;
        }
    }
}
