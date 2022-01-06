# QQ机器人框架

[![NuGet](https://img.shields.io/nuget/v/QQChannelBot.svg)](https://www.nuget.org/packages/QQChannelBot) [![NuGet downloads](https://img.shields.io/nuget/dt/QQChannelBot)](https://www.nuget.org/packages/QQChannelBot)

使用.NET6技术封装的QQ机器人通信框架，无需任何第三方依赖。   
傻瓜式封装，极简的调用接口，让新手也能快速入门！

## 使用说明

1.配置日志输出等级（默认值：LogLevel.Info）
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
    var sdk = typeof(BotClient).Assembly.GetName();
    Console.Title = $"{sender?.UserName}{(bot.DebugBot ? "-DEBUG" : "")} <{sender?.Id}> - SDK版本：{sdk.Name}_{sdk.Version}";
};
```

4.如何接收 @机器人 消息（注：OnAtMessage 事件已弃用）
```cs
// 订阅消息创建事件,处理所有收到的消息
// 注1：如果该消息包含 @机器人 标签，则isAt=true
// 注2：被 AddCommand 指令命中的消息不会出现在这里
// sender是Message对象(自动内嵌BotClient对象，以支持快速回复)
// 这里示例快速回复用户的AT消息，并修改 @机器人 为 @用户
bot.OnMsgCreate += async (sender, isAt) =>
{
    if (isAt)
    {
        string replyMsg = sender.Content.Replace(MsgTag.User(bot.Info!.Id), MsgTag.User(sender.Author.Id));
        await sender.ReplyAsync(replyMsg);
    }
};
```

5.注册自定义消息命令
```cs
// 注册自定义命令，这里让机器人复读用户的消息
// 如:用户发送 @机器人 复读 123
// 　 机器回复 123
bot.AddCommand(new Command("复读", async (sender, msg) =>
{
    await sender.ReplyAsync($"{MsgTag.User(sender.Author.Id)} {msg}");
}));
```

6.启动机器人(开始运行并监听频道消息)
```cs
bot.Start();
```

## 以下是API调用玩法示例
#### 注：AddCommand注册指令；[点此查看Command指令对象详细参数](https://github.com/Antecer/QQChannelBot/blob/main/QQChannelBot/Bot/ObjectClass.cs)
```
// 注册自定义命令，这里测试embed消息 ( 实现功能为获取用户信息，指令格式： @机器人 UserInfo @用户 )
bot.AddCommand(new Command("用户信息", async (sender, msg) =>
{
    string userId = Regex.IsMatch(msg, @"<@!(\d+)>") ? Regex.Match(msg, @"<@!(\d+)>").Groups[1].Value : sender.Author.Id;
    Member member = await bot.GetMemberAsync(sender.GuildId, userId) ?? new()
    {
        User = sender.Author,
        Roles = sender.Member.Roles,
        JoinedAt = sender.Member.JoinedAt,
    };
    List<Role>? roles = await bot.GetRolesAsync(sender.GuildId);
    List<string> roleNames = roles?.Where(role => member.Roles.Contains(role.Id)).Select(role => role.Name).ToList() ?? new List<string> { "无权获取身份组列表" };
    MsgEmbed ReplyEmbed = new MsgEmbed()
    {
        Title = member.User!.UserName,
        Prompt = $"{member.User.UserName} 的信息卡",
        Thumbnail = member.User.Avatar
    }
    .AddLine($"用户昵称：{member.Nick}")
    .AddLine($"账户类别：{(member.User.Bot ? "机器人" : "人类")}")
    .AddLine($"角色分类：{string.Join("、", roleNames)}")
    .AddLine($"加入时间：{member.JoinedAt!.Remove(member.JoinedAt.IndexOf('+'))}");
    await sender.ReplyAsync(ReplyEmbed);
}));
// 指令格式：@机器人 创建公告 公告内容
bot.AddCommand(new Command("创建公告", async (sender, args) =>
{
    if (string.IsNullOrWhiteSpace(args))
    {
        await sender.ReplyAsync($"{MsgTag.User(sender.Author.Id)} 未指定公告内容!\n正确格式：@机器人 创建公告 公告内容");
        return;
    }
    Message? sendmsg = await sender.ReplyAsync(args);
    await bot.CreateAnnouncesAsync(sendmsg!);
}, needAdmin: true));
// 指令格式：@机器人 删除公告
bot.AddCommand(new Command("删除公告", async (sender, args) =>
{
    await bot.DeleteAnnouncesAsync(sender.ChannelId);
}, needAdmin: true));
// 指令格式：@机器人 创建全局公告 公告内容
bot.AddCommand(new Command("创建全局公告", async (sender, args) =>
{
    if (string.IsNullOrWhiteSpace(args))
    {
        await sender.ReplyAsync($"{MsgTag.User(sender.Author.Id)} 未指定公告内容!\n正确格式：@机器人 创建全局公告 公告内容");
        return;
    }
    Message? sendmsg = await sender.ReplyAsync(args);
    await bot.CreateAnnouncesGlobalAsync(sendmsg!);
}, needAdmin: true));
// 指令格式：@机器人 删除全局公告
bot.AddCommand(new Command("删除全局公告", async (sender, args) =>
{
    await bot.DeleteAnnouncesGlobalAsync(sender.ChannelId);
}, needAdmin: true));
// 指令格式：@机器人 禁言 @用户 10天(或：到2077-12-12 23:59:59)
bot.AddCommand(new Command("禁言", async (sender, args) =>
{
    Match userIdMatcher = Regex.Match(args, @"<@!(\d+)>");
    string? userId = userIdMatcher.Success ? userIdMatcher.Groups[1].Value : null;
    if (userId == null)
    {
        await sender.ReplyAsync($"{MsgTag.User(sender.Author.Id)} 未指定禁言的用户!\n正确格式：@机器人 禁言 @用户 禁言时间");
        return;
    }
    Match tsm = Regex.Match(args, @"(\d{4})[-年](\d\d)[-月](\d\d)[\s日]*(\d\d)[:点时](\d\d)[:分](\d\d)秒?");
    if (tsm.Success)
    {
        string timeStampStr = tsm.Groups[0].Value;
        bool isOk = await bot.MuteMemberAsync(sender.GuildId, userId, new MuteMaker(timeStampStr));
        if (isOk) await sender.ReplyAsync($"{sender.Mentions!.Find(u => u.Id == userId)?.UserName} 已被禁言，解除时间：{timeStampStr}");
        else await sender.ReplyAsync($"禁言失败,可能没有权限!");
    }
    else
    {
        Match tdm = Regex.Match(args, @"(\d+)\s*(年|星期|周|日|天|小?时|分钟?|秒钟?)");
        bool isOk = await bot.MuteMemberAsync(sender.GuildId, userId, tdm.Success ? new MuteMaker(tdm.Groups[0].Value) : new MuteTime(60));
        if (isOk) await sender.ReplyAsync($"{sender.Mentions!.Find(u => u.Id == userId)?.UserName} 已被禁言{(tdm.Success ? tdm.Groups[0] : "1分钟")}");
        else await sender.ReplyAsync($"禁言失败,可能没有权限!");
    }
}, needAdmin: true));
// 指令格式：@机器人 解除禁言 @用户
bot.AddCommand(new Command("解除禁言", async (sender, args) =>
{
    Match userIdMatcher = Regex.Match(args, @"<@!(\d+)>");
    string? userId = userIdMatcher.Success ? userIdMatcher.Groups[1].Value : null;
    if (userId == null)
    {
        await sender.ReplyAsync($"{MsgTag.User(sender.Author.Id)} 未指定解禁的用户!\n正确格式：@机器人 解禁 @用户");
        return;
    }
    bool isOk = await bot.MuteMemberAsync(sender.GuildId, userId, new MuteTime(0));
    if (isOk) await sender.ReplyAsync($"{sender.Mentions!.Find(u => u.Id == userId)?.UserName} 已解除禁言");
    else await sender.ReplyAsync($"解除禁言失败,可能没有权限!");
}, needAdmin: true));
// 指令格式：@机器人 全体禁言 10天(或：到2077年12月12日23点59分59秒)
bot.AddCommand(new Command("全员禁言", async (sender, args) =>
{
    Match tsm = Regex.Match(args, @"(\d{4})[-年](\d\d)[-月](\d\d)[\s日]*(\d\d)[:点时](\d\d)[:分](\d\d)秒?");
    if (tsm.Success)
    {
        string timeStampStr = tsm.Groups[0].Value;
        bool isOk = await bot.MuteGuildAsync(sender.GuildId, new MuteMaker(timeStampStr));
        if (isOk) await sender.ReplyAsync($"已启用全员禁言，解除时间：{timeStampStr}");
        else await sender.ReplyAsync($"全员禁言失败,可能没有权限!");
    }
    else
    {
        Match tdm = Regex.Match(args, @"(\d+)\s*(年|星期|周|日|天|小?时|分钟?|秒钟?)");
        bool isOk = await bot.MuteGuildAsync(sender.GuildId, tdm.Success ? new MuteMaker(tdm.Groups[0].Value) : new MuteTime(60));
        if (isOk) await sender.ReplyAsync($"已启用全员禁言{(tdm.Success ? tdm.Groups[0] : "1分钟")}");
        else await sender.ReplyAsync($"全员禁言失败,可能没有权限!");
    }
}, needAdmin: true));
// 指令格式：@机器人 解除全体禁言
bot.AddCommand(new Command("解除全员禁言", async (sender, args) =>
{
    bool isOk = await bot.MuteGuildAsync(sender.GuildId, new MuteTime(0));
    if (isOk) await sender.ReplyAsync($"已解除全员禁言");
    else await sender.ReplyAsync($"解除全员禁言失败,可能没有权限!");
}, needAdmin: true));
```

## Ark模板消息构建方法
#### 注：模板消息的构造支持多种方式，以下尽量展示了不同的构造方式，也可以混合使用各种构造方式。
```cs
bot.AddCommand(new Command("ark测试", async (sender, args) =>
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
}));
```

### 更多玩法请参考 [Benchmarks](https://github.com/Antecer/QQChannelBot/blob/main/QQChannelBot/Tools/Benchmarks.cs)