#QQ机器人框架

[![NuGet](https://img.shields.io/nuget/v/QQChannelBot.svg)](https://www.nuget.org/packages/QQChannelBot) [![NuGet downloads](https://img.shields.io/nuget/dt/QQChannelBot)](https://www.nuget.org/packages/QQChannelBot)

使用.NET6技术封装的QQ机器人通信框架

##使用说明

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
