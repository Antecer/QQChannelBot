# QQ机器人框架

[![NuGet](https://img.shields.io/nuget/v/QQChannelBot.svg)](https://www.nuget.org/packages/QQChannelBot) [![NuGet downloads](https://img.shields.io/nuget/dt/QQChannelBot)](https://www.nuget.org/packages/QQChannelBot)

使用.NET6技术封装的QQ机器人通信框架，无需任何第三方依赖。
傻瓜式封装，极简的调用接口，让新手也能快速入门！

## 使用说明

1.配置日志输出等级
``` cs
Log.LogLevel = LogLevel.Debug;
```

2.创建机器人, 配置参数请查阅 [QQ机器管理后台](https://bot.q.qq.com/#/developer/developer-setting)
```cs
BotClient bot = new(new()
{
    BotAppId = 开发者ID,
    BotToken = 机器人令牌,
    BotSecret = 机器人密钥
});
```

3.订阅 OnReady 事件，这里根据机器人信息修改控制台标题
```
bot.OnReady += (sender) =>
{
    Console.Title = $"QCBot: {sender?.UserName}<{sender?.Id}>";
};
```

4.如何接收 @机器人 消息
```cs
// 订阅 OnAtMessage 事件，处理所有收到的 @机器人 消息
// 注1：被 AddCommand 命令匹配的消息不会出现在这里
// 注2：若要接收服务器推送的所有消息，请订阅 OnDispatch 事件
// sender是Message对象(自动内嵌BotClient对象，以支持快速回复)
// 这里示例快速回复用户的AT消息，并修改 @机器人 为 @用户
bot.OnAtMessage += async (sender) =>
{
    string replyMsg = sender.Content.TrimStartString(MsgTag.UserTag(bot.Info?.Id), MsgTag.UserTag(sender.Author.Id));
    await sender.ReplyAsync()
};
```

5.注册自定义消息命令
```cs
// 注册自定义命令，这里让机器人复读用户的消息
// 如:用户发送 @机器人 复读 123
// 　 机器回复 123
bot.AddCommand("复读", async (sender, msg) =>
{
    await sender.ReplyAsync(msg);
});
```

6.启动机器人(开始运行并监听频道消息)
```cs
bot.Start();
```

## 以下是API调用玩法示例
#### 注：AddCommand注册普通指令，任何成员可触发；AddCommandSuper注册管理员指令，仅频道主、管理员、子频道管理员可触发。
```
// 注册自定义命令，这里测试embed消息 ( 实现功能为获取用户信息，指令格式： @机器人 UserInfo @用户 )
bot.AddCommand("用户信息", async (sender, msg) =>
{
    string userId = Regex.IsMatch(msg, @"<@!(\d+)>") ? Regex.Match(msg, @"<@!(\d+)>").Groups[1].Value : sender.Author.Id;
    Member member = await bot.GetGuildMemberAsync(sender.GuildId, userId) ?? new()
    {
        User = sender.Author,
        Roles = sender.Member.Roles,
        JoinedAt = sender.Member.JoinedAt,
    };
    GuildRoles? grs = await bot.GetGuildRolesAsync(sender.GuildId);
    var roles = grs?.Roles.Where(gr => member.Roles.Contains(gr.Id)).Select(gr => gr.Name).ToList() ?? new List<string> { "无权获取身份组列表" };
    MsgEmbed ReplyEmbed = new MsgEmbed()
    {
        Title = member.User!.UserName,
        Prompt = $"{member.User.UserName} 的信息卡",
        Thumbnail = member.User.Avatar
    }
    .AddLine($"用户昵称：{member.Nick}")
    .AddLine($"账户类别：{(member.User.Bot ? "机器人" : "人类")}")
    .AddLine($"角色分类：{string.Join("、", roles)}")
    .AddLine($"加入时间：{member.JoinedAt!.Remove(member.JoinedAt.IndexOf('+'))}");
    await sender.ReplyAsync(ReplyEmbed);
});
// 指令格式：@机器人 创建公告 公告内容
bot.AddCommandSuper("创建公告", async (sender, msg) =>
{
    Message? sendmsg = await sender.ReplyAsync(msg);
    await bot.CreateAnnouncesAsync(sendmsg!);
});
// 指令格式：@机器人 删除公告
bot.AddCommandSuper("删除公告", async (sender, msg) =>
{
    await bot.DeleteAnnouncesAsync(sender.ChannelId);
});
// 指令格式：@机器人 创建全局公告 公告内容
bot.AddCommandSuper("创建全局公告", async (sender, msg) =>
{
    Message? sendmsg = await sender.ReplyAsync(msg);
    await bot.CreateAnnouncesGlobalAsync(sendmsg!);
});
// 指令格式：@机器人 删除全局公告
bot.AddCommandSuper("删除全局公告", async (sender, msg) =>
{
    await bot.DeleteAnnouncesGlobalAsync(sender.ChannelId);
});
// 指令格式：@机器人 禁言 @用户 10天(或：到2077-12-12 23:59:59)
bot.AddCommandSuper("禁言", async (sender, msg) =>
{
    Match userIdMatcher = Regex.Match(msg, @"<@!(\d+)>");
    string? userId = userIdMatcher.Success ? userIdMatcher.Groups[1].Value : null;
    if (userId == null)
    {
        await sender.ReplyAsync($"{MsgTag.UserTag(sender.Author.Id)} 未指定禁言的用户!");
        return;
    }
    Match tsm = Regex.Match(msg, @"(\d{4})[-年](\d\d)[-月](\d\d)[\s日]*(\d\d)[:点时](\d\d)[:分](\d\d)秒?");
    if (tsm.Success)
    {
        string timeStampStr = tsm.Groups[0].Value;
        await bot.MuteMemberAsync(sender.GuildId, userId, new MuteMaker(timeStampStr));
        await sender.ReplyAsync($"{sender.Mentions!.Find(u => u.Id == userId)?.UserName} 已被禁言，解除时间：{timeStampStr}");
    }
    else
    {
        Match tdm = Regex.Match(msg, @"(\d+)\s*(年|星期|周|日|天|小?时|分钟?|秒钟?)");
        await bot.MuteMemberAsync(sender.GuildId, userId, tdm.Success ? new MuteMaker(tdm.Groups[0].Value) : new MuteTime(60));
        await sender.ReplyAsync($"{sender.Mentions!.Find(u => u.Id == userId)?.UserName} 已被禁言{(tdm.Success ? tdm.Groups[0] : "1分钟")}");
    }
});
// 指令格式：@机器人 解除禁言 @用户
bot.AddCommandSuper("解除禁言", async (sender, msg) =>
{
    Match userIdMatcher = Regex.Match(msg, @"<@!(\d+)>");
    string? userId = userIdMatcher.Success ? userIdMatcher.Groups[1].Value : null;
    if (userId == null)
    {
        await sender.ReplyAsync($"{MsgTag.UserTag(sender.Author.Id)} 未指定解禁的用户!");
        return;
    }
    await bot.MuteMemberAsync(sender.GuildId, userId, new MuteTime(0));
    await sender.ReplyAsync($"{sender.Mentions!.Find(u => u.Id == userId)?.UserName} 已解除禁言");
});
// 指令格式：@机器人 全体禁言 10天(或：到2077年12月12日23点59分59秒)
bot.AddCommandSuper("全员禁言", async (sender, msg) =>
{
    Match tsm = Regex.Match(msg, @"(\d{4})[-年](\d\d)[-月](\d\d)[\s日]*(\d\d)[:点时](\d\d)[:分](\d\d)秒?");
    if (tsm.Success)
    {
        string timeStampStr = tsm.Groups[0].Value;
        await bot.MuteGuildAsync(sender.GuildId, new MuteMaker(timeStampStr));
        await sender.ReplyAsync($"已启用全员禁言，解除时间：{timeStampStr}");
    }
    else
    {
        Match tdm = Regex.Match(msg, @"(\d+)\s*(年|星期|周|日|天|小?时|分钟?|秒钟?)");
        await bot.MuteGuildAsync(sender.GuildId, tdm.Success ? new MuteMaker(tdm.Groups[0].Value) : new MuteTime(60));
        await sender.ReplyAsync($"已启用全员禁言{(tdm.Success ? tdm.Groups[0] : "1分钟")}");
    }
});
// 指令格式：@机器人 解除全体禁言
bot.AddCommandSuper("解除全员禁言", async (sender, msg) =>
{
    await bot.MuteGuildAsync(sender.GuildId, new MuteTime(0));
    await sender.ReplyAsync($"已解除全员禁言");
});
```

## Ark模板消息构建方法
#### 注：模板消息的构造支持多种方式，以下尽量展示了不同的构造方式，也可以混合使用各种构造方式。
```cs
bot.AddCommand("ark测试", async (sender, msg) =>
{
    // Ark23测试通过
    await sender.ReplyAsync(new MsgArk23()
        .SetDesc("描述")
        .SetPrompt("提示消息")
        .AddLine("第一行内容")
        .AddLine("第二行内容")
        .AddLine("百度")
        .AddLine("淘宝")
        .AddLine("腾讯")
        .AddLine("微软")
        .AddLine("最后一行"));

    // Ark24测试通过
    await sender.ReplyAsync(new MsgArk24()
        .SetDesc("描述")
        .SetPrompt("提示")
        .SetTitle("标题")
        .SetMetaDesc("详情")
        .SetImage("")
        .SetLink("")
        .SetSubTitle("子标题"));

    // Ark34测试通过
    await sender.ReplyAsync(new MsgArk34()
    {
        Desc = "描述",
        Prompt = "提示",
        MetaTitle = "标题",
        MetaDesc = "详情",
        MetaIcon = "",
        MetaPreview = "",
        MetaUrl = ""
    });

    // Ark37测试通过
    await sender.ReplyAsync(new MsgArk37("提示", "标题", "子标题"));

    // 图片消息测试通过
    await sender.ReplyAsync(new MsgImage("http://thirdqq.qlogo.cn/g?b=oidb&k=cSmKqHHUOtQicbia4JGgnkJA&s=0&t=1555550077"));

    // 文字+图片消息测试通过
    await sender.ReplyAsync(new MsgDescImg("文字内容", "http://thirdqq.qlogo.cn/g?b=oidb&k=cSmKqHHUOtQicbia4JGgnkJA&s=0&t=1555550077"));
});
```