# QQ机器人框架

[![NuGet](https://img.shields.io/nuget/v/QQChannelBot.svg)](https://www.nuget.org/packages/QQChannelBot) [![NuGet downloads](https://img.shields.io/nuget/dt/QQChannelBot)](https://www.nuget.org/packages/QQChannelBot)

使用.NET6技术封装的QQ机器人通信框架

## 使用说明

1.配置日志输出等级
``` cs
Log.LogLevel = LogLevel.Debug;
```

2.创建机器人, 配置参数请查阅 [QQ机器管理后台](https://bot.q.qq.com/#/developer/developer-setting)
```cs
ChannelBot bot = new(new()
{
    BotAppId = 开发者ID,
    BotToken = 机器人令牌,
    BotSecret = 机器人密钥
});
```

3.注册自定义消息命令
```cs
// 注册自定义命令，这里让机器人复读用户的消息
// 如:用户发送 @机器人 复读 123
// 　 机器人将回复 123
bot.AddCommand("复读", async (sender, e, msg) =>
{
    await sender.SendMessageAsync(e.ChannelId, new MsgNormal(e.Id)
    {
        Content = msg
    }.Body);
});
```
4.如何接收 @机器人 消息
```cs
// 订阅 OnAtMessage 事件，处理所有收到的 @机器人 消息
// 注1：被 AddCommand 命令匹配的消息不会出现在这里
// 注2：若要接收服务器推送的所有消息，请订阅 OnDispatch 事件
// sender是ChannelBot对象,e是Message对象,type是消息类型(暂无用,后面可能删除)
bot.OnAtMessage += async (sender, e, type) =>
{
};
```
5.启动机器人(开始运行并监听频道消息)
```cs
bot.Start();
```

## 以下是API调用玩法示例
#### 注：所有API调用均未对指令发出者做鉴权判断，使用者调用时需要自己加入权限判断。
```
// 如：限制 仅管理员、频道主、子频道管理员 能触发指令
bot.AddCommand("创建公告", async (sender, e, msg) =>
{
    if (!e.Member.Roles.Any(r => "234".Contains(r))) return; // 如果发送指令的用户没有管理权限，就不运行后续代码
    Message? sendmsg = await bot.SendMessageAsync(e.ChannelId, msg, e.Id);
    await bot.CreateAnnouncesAsync(sendmsg!);
});
```
```cs
// 指令格式：@机器人 创建公告 公告内容
bot.AddCommand("创建公告", async (sender, e, msg) =>
{
    Message? sendmsg = await bot.SendMessageAsync(e.ChannelId, msg, e.Id);
    await bot.CreateAnnouncesAsync(sendmsg!);
});
// 指令格式：@机器人 删除公告
bot.AddCommand("删除公告", async (sender, e, msg) =>
{
    await bot.DeleteAnnouncesAsync(e.ChannelId);
});
// 指令格式：@机器人 创建全局公告 公告内容
bot.AddCommand("创建全局公告", async (sender, e, msg) =>
{
    Message? sendmsg = await bot.SendMessageAsync(e.ChannelId, msg, e.Id);
    await bot.CreateAnnouncesGlobalAsync(sendmsg!);
});
// 指令格式：@机器人 删除全局公告
bot.AddCommand("删除全局公告", async (sender, e, msg) =>
{
    await bot.DeleteAnnouncesGlobalAsync(e.ChannelId);
});
// 指令格式：@机器人 禁言 @用户 10天
// 或使用格式：@机器人 禁言 @用户 到 2077-12-12 23:59:59
bot.AddCommand("禁言", async (sender, e, msg) =>
{
    string? userId = Regex.IsMatch(msg, @"<@!(\d+)>") ? Regex.Match(msg, @"<@!(\d+)>").Groups[1].Value : null;
    if (userId == null)
    {
        await sender.SendMessageAsync(e.ChannelId, new MessageToCreate($"{MsgTag.UserTag(e.Author.Id)} 未指定禁言的用户!", e.Id));
        return;
    }
    Match? timeGroups = Regex.IsMatch(msg, @"(\d+)(天|小时|时|分|秒)") ? Regex.Match(msg, @"(\d+)(天|小时|时|分|秒)") : null;
    string timeType = timeGroups?.Groups[2].Value ?? "秒";
    int timeDelay = int.Parse(timeGroups?.Groups[1].Value ?? "60");
    int delaySecond = timeType switch
    {
        "天" => 60 * 60 * 24,
        "小时" => 60 * 60,
        "时" => 60 * 60,
        "分" => 60,
        _ => 1
    } * timeDelay;
    string? timsTamp = Regex.IsMatch(msg, @"\d{4}-\d\d-\d\d\s+\d\d:\d\d:\d\d") ? Regex.Match(msg, @"\d{4}-\d\d-\d\d\s+\d\d:\d\d:\d\d").Groups[0].Value : null;
    await bot.MuteMemberAsync(e.GuildId, userId, timsTamp != null ? new MuteTime(timsTamp) : new MuteTime(delaySecond));
    await sender.SendMessageAsync(e.ChannelId, new MessageToCreate()
    {
        Content = $"{e.Mentions!.Find(u => u.Id == userId)?.UserName} 已被禁言{(timsTamp != null ? "，解除时间：" + timsTamp : timeDelay.ToString() + timeType)}",
        MsgId = e.Id
    });
});
// 指令格式：@机器人 解除禁言 @用户
bot.AddCommand("解除禁言", async (sender, e, msg) =>
{
    string? userId = Regex.IsMatch(msg, @"<@!(\d+)>") ? Regex.Match(msg, @"<@!(\d+)>").Groups[1].Value : null;
    if (userId == null)
    {
        await sender.SendMessageAsync(e.ChannelId, new MessageToCreate($"{MsgTag.UserTag(e.Author.Id)} 未指定解除的用户!", e.Id));
        return;
    }
    await bot.MuteMemberAsync(e.GuildId, userId, new MuteTime(0));
    await sender.SendMessageAsync(e.ChannelId, new MessageToCreate($"{e.Mentions!.Find(u => u.Id == userId)?.UserName} 已解除禁言", e.Id));
});
// 指令格式：@机器人 全体禁言 10天
// 或使用格式：@机器人 全体禁言 到 2077-12-12 23:59:59
bot.AddCommand("全体禁言", async (sender, e, msg) =>
{
    Match? timeGroups = Regex.IsMatch(msg, @"(\d+)(天|小时|时|分|秒)") ? Regex.Match(msg, @"(\d+)(天|小时|时|分|秒)") : null;
    string timeType = timeGroups?.Groups[2].Value ?? "秒";
    int timeDelay = int.Parse(timeGroups?.Groups[1].Value ?? "60");
    int delaySecond = timeType switch
    {
        "天" => 60 * 60 * 24,
        "小时" => 60 * 60,
        "时" => 60 * 60,
        "分" => 60,
        _ => 1
    } * timeDelay;
    string? timsTamp = Regex.IsMatch(msg, @"\d{4}-\d\d-\d\d\s+\d\d:\d\d:\d\d") ? Regex.Match(msg, @"\d{4}-\d\d-\d\d\s+\d\d:\d\d:\d\d").Groups[0].Value : null;
    await bot.MuteGuildAsync(e.GuildId, timsTamp != null ? new MuteTime(timsTamp) : new MuteTime(delaySecond));
    await sender.SendMessageAsync(e.ChannelId, new MessageToCreate()
    {
        Content = $"{e.Author.UserName} 已启用全员禁言{(timsTamp != null ? "，解除时间：" + timsTamp : timeDelay.ToString() + timeType)}",
        MsgId = e.Id
    });
});
// 指令格式：@机器人 解除全体禁言
bot.AddCommand("解除全体禁言", async (sender, e, msg) =>
{
    await bot.MuteGuildAsync(e.GuildId, new MuteTime(0));
    await sender.SendMessageAsync(e.ChannelId, new MessageToCreate($"已解除全员禁言", e.Id));
});
```
