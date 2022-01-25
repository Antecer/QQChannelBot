# QQ机器人框架

[![NuGet](https://img.shields.io/nuget/v/QQChannelBot.svg)](https://www.nuget.org/packages/QQChannelBot)
[![NuGet downloads](https://img.shields.io/nuget/dt/QQChannelBot)](https://www.nuget.org/packages/QQChannelBot)
[![License](https://img.shields.io/github/license/Antecer/QQChannelBot)](https://www.nuget.org/packages/QQChannelBot)
[![Platform](https://img.shields.io/badge/platform-.net6-blue)](https://www.nuget.org/packages/QQChannelBot)
[![Benchmarks](https://img.shields.io/badge/benchmarks-%E2%9C%9434%20%7C%20%E2%9C%983%20%7C%20%E2%9E%9C1-orange)](https://github.com/Antecer/QQChannelBot/blob/main/QQChannelBot/Tools/Benchmarks.cs)

使用.NET6技术封装的QQ机器人通信框架，无需任何第三方依赖。   
傻瓜式封装，极简的调用接口，让新手也能快速入门！

## 使用说明

1.配置日志输出等级（默认值：LogLevel.INFO）
``` cs
Log.LogLevel = LogLevel.DEBUG;
```

2.创建机器人, 配置参数请查阅 [QQ机器管理后台](https://bot.q.qq.com/#/developer/developer-setting)
```cs
// 配置鉴权信息
BotClient bot = new(new()
{
    BotAppId = 开发者ID,
    BotToken = 机器人令牌,
    BotSecret = 机器人密钥
});
// 配置频道事件监听
bot.Intents = Intents.Public;
// 配置自定义指令前缀（私域机器人可自定义前缀触发指令匹配功能，默认值"/"；公域机器人无需配置）
bot.CommandPrefix = "/";
// 配置消息过滤器（结果为true的消息将被拦截）
bot.MessageFilter = (sender) =>
{
    return sender.ChannelId != "2500191";
};
```

3.框架事件订阅演示
```
// 订阅 OnReady 事件，这里根据机器人信息修改控制台标题
bot.OnReady += (sender) =>
{
    var sdk = typeof(BotClient).Assembly.GetName();
    consoleTitle = $"{sender.UserName}{(bot.PrivateGuilds.Any() ? "_Private" : "_Public")} <{sender.Id}> - SDK版本：{sdk.Name}_{sdk.Version}；　连接状态：";
    Console.Title = consoleTitle;
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
        string replyMsg = sender.Content.Replace(sender.Bot.Info.Tag, sender.Author.Tag);
        await sender.ReplyAsync(replyMsg);
    }
};
```

5.注册自定义消息命令
```cs
// 注册自定义命令，这里让机器人复读用户的消息
// 如:用户发送 @机器人 复读 123
// 　 机器回复 @用户 123
bot.AddCommand(new Command("复读", async (sender, msg) =>
{
    await sender.ReplyAsync($"{sender.Author.Tag} {msg}");
}));
```

6.启动机器人(开始运行并监听频道消息)
```cs
bot.Start();
```

## 以下是API调用玩法示例
#### 注：AddCommand注册指令；[点此查看Command指令对象详细参数](https://github.com/Antecer/QQChannelBot/blob/main/QQChannelBot/Bot/Command.cs)
```
// 指令格式：@机器人 菜单
// 注：这里排除了Command属性Note有内容的指令
bot.AddCommand(new Command("菜单", async (sender, args) =>
{
    await sender.ReplyAsync(string.join('、', sender.Bot.GetCommands.Where(cmd => string.IsNullOrWhiteSpace(cmd.Note)).Select(cmd => cmd.Name).ToList()));
}, note: "hide"));
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
    if (args.IsBlank())
    {
        await sender.ReplyAsync($"{sender.Author.Tag} 未指定公告内容！正确格式：\n创建公告 公告内容");
        return;
    }
    Message? sendmsg = await sender.ReplyAsync(args);
    Announces? announces = null;
    if (sendmsg != null) announces = await sender.CreateAnnouncesAsync(sendmsg);
    if (sendmsg == null || announces == null) await sender.ReplyAsync($"{sender.Author.Tag} 公告创建失败，{sender.Bot.Info.UserName} 无权执行该操作！");
    else await sender.ReplyAsync($"{sender.Author.Tag} 公告已发布");
}, null, true));
// 指令格式：@机器人 删除公告
bot.AddCommand(new Command("删除公告", async (sender, args) =>
{
    if (await sender.DeleteAnnouncesAsync()) await sender.ReplyAsync($"{sender.Author.Tag} 公告已删除！");
    else await sender.ReplyAsync($"{sender.Bot.Info.UserName} 无权删除公告！");
}, null, true));
// 指令格式：@机器人 创建全局公告 公告内容
bot.AddCommand(new Command("创建全局公告", async (sender, args) =>
{
    if (args.IsBlank())
    {
        await sender.ReplyAsync($"{sender.Author.Tag} 未指定公告内容！正确格式：\n创建全局公告 公告内容");
        return;
    }
    Message? sendmsg = await sender.ReplyAsync(args);
    Announces? announces = null;
    if (sendmsg != null) announces = await sender.CreateAnnouncesGlobalAsync(sendmsg);
    if (sendmsg == null || announces == null) await sender.ReplyAsync($"{sender.Author.Tag} 全局公告创建失败，{sender.Bot.Info.UserName} 无权执行该操作！");
    else await sender.ReplyAsync($"{sender.Author.Tag} 全局公告已发布");
}, null, true));
// 指令格式：@机器人 删除全局公告
bot.AddCommand(new Command("删除全局公告", async (sender, args) =>
{
    if (await sender.DeleteAnnouncesGlobalAsync()) await sender.ReplyAsync($"{sender.Author.Tag} 全局公告已删除！");
    else await sender.ReplyAsync($"{sender.Bot.Info.UserName} 无权删除全局公告！");
}, null, true));
// 指令格式：@机器人 禁言 @用户 10天(或：到2077-12-12 23:59:59)
bot.AddCommand(new Command("禁言", async (sender, args) =>
{
    List<User>? users = sender.Mentions?.ToHashSet().ToList();
    users?.RemoveAll(user => user.Id == sender.Bot.Info.Id); // 排除机器人自己
    if (users?.Any() != true)
    {
        await sender.ReplyAsync($"{sender.Author.Tag} 未指定禁言的用户！正确格式：\n禁言 @用户 禁言时间(年|星期|周|日|天|小时|分|秒；默认单位分钟)\n禁言 禁言时间 @用户(可多个)");
        return;
    }
    string? muteMakerAfter = null;
    string muteMakerDelay = "1分钟";
    args = Regex.Replace(args, @"<@!\d+>", "");
    Match tsm = Regex.Match(args, @"(\d{4})[-年](\d\d)[-月](\d\d)[\s日]*(\d\d)[:点时](\d\d)[:分](\d\d)秒?");
    if (tsm.Success) muteMakerAfter = tsm.Groups[0].Value;
    else
    {
        Match tdm = Regex.Match(args, @"(\d+)\s*(年|星期|周|日|天|小?时|分钟?|秒钟?)");
        if (tdm.Success) muteMakerDelay = tdm.Groups[0].Value;
        else
        {
            Match ttm = Regex.Match(args, @"\d+");
            if (ttm.Success) muteMakerDelay = ttm.Groups[0].Value + "分钟";
        }
    }
    string muteTimeAt = muteMakerAfter != null ? $"解除时间：{muteMakerAfter}" : $"持续时间：{muteMakerDelay}";
    MuteMaker muteMaker = new(muteMakerAfter ?? muteMakerDelay);
    foreach (var user in users)
    {
        if (await sender.MuteMemberAsync(user, muteMaker))
        {
            await sender.ReplyAsync($"{user.UserName} 已被禁言，{muteTimeAt}");
        }
        else await sender.ReplyAsync($"禁言失败，{sender.Bot.Info.UserName} 无权禁言用户：{user.UserName}");
    }
}, new Regex(@"^禁言(?=(\d|\s|<@!\d+>)|$)"), true));
// 指令格式：@机器人 解除禁言 @用户
bot.AddCommand(new Command("解除禁言", async (sender, args) =>
{
    List<User>? users = sender.Mentions;
    users?.RemoveAll(user => user.Id == sender.Bot.Info.Id); // 排除机器人自己
    if (users?.Any() != true)
    {
        await sender.ReplyAsync($"{sender.Author.Tag} 未指定解禁的用户!\n正确格式：解除禁言 @用户(可多个)");
        return;
    }
    MuteTime muteTime = new(0);
    foreach (var user in users)
    {
        if (await sender.MuteMemberAsync(user, muteTime))
        {
            await sender.ReplyAsync($"{user.UserName} 已解除禁言");
        }
        else await sender.ReplyAsync($"解禁失败，{sender.Bot.Info.UserName} 无权解禁用户：{user.UserName}");
    }
}, new Regex(@"^(解除|取消)禁言(?=(\s|<@!\d+>)|$)"), true));
// 指令格式：@机器人 全体禁言 10天(或：到2077年12月12日23点59分59秒)
bot.AddCommand(new Command("全员禁言", async (sender, args) =>
{
    Match tsm = Regex.Match(args, @"(\d{4})[-年](\d\d)[-月](\d\d)[\s日]*(\d\d)[:点时](\d\d)[:分](\d\d)秒?");
    string? muteMakerAfter = null;
    string muteMakerDelay = "1分钟";
    if (tsm.Success) muteMakerAfter = tsm.Groups[0].Value;
    else
    {
        Match tdm = Regex.Match(args, @"(\d+)\s*(年|星期|周|日|天|小?时|分钟?|秒钟?)");
        if (tdm.Success) muteMakerDelay = tdm.Groups[0].Value;
        else
        {
            await sender.ReplyAsync($"{sender.Author.Tag} 时间格式不正确！正确格式：\n全员禁言 禁言时间(年|星期|周|日|天|小时|分|秒)\n全员禁言 禁言时间(xxxx年xx月xx日xx点(时)xx分xx秒)");
            return;
        }
    }
    string muteTimeAt = muteMakerAfter != null ? $"解除时间：{muteMakerAfter}" : $"持续时间：{muteMakerDelay}";
    if (await sender.MuteGuildAsync(new MuteMaker(muteMakerAfter ?? muteMakerDelay)))
    {
        await sender.ReplyAsync($"已启用全员禁言，{muteTimeAt}");
    }
    else await sender.ReplyAsync($"全员禁言失败，{sender.Bot.Info.UserName} 无权启用全员禁言!");
}, null, true));
// 指令格式：@机器人 解除全体禁言
bot.AddCommand(new Command("解除全员禁言", async (sender, args) =>
{
    bool isOk = await sender.MuteGuildAsync(new MuteTime(0));
    await sender.ReplyAsync(isOk ? "已解除全员禁言" : $"解除全员禁言失败，{sender.Bot.Info.UserName} 无权解除全员禁言!");
}, new Regex(@"^(解除|取消)全员禁言$"), true));
// 指令格式：@机器人 创建身份组
bot.AddCommand(new Command("创建角色", async (sender, args) =>
{
    Match roleParams = Regex.Match(args, @"([^#;\s]+);(#[0-9a-fA-F]+)(;分组)?");
    if (!roleParams.Success)
    {
        await sender.ReplyAsync($"{sender.Author.Tag} 参数不正确！正确格式(使用';'连接参数)：\n￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣\n创建角色 名称;颜色(格式#FFFFFF);分组(可选)\n＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿＿\n例：创建角色 新角色;#FF7788;分组\n　　新建角色 新角色2;#F0F");
        return;
    }
    string roleName = roleParams.Groups[1].Value;
    string roleColor = roleParams.Groups[2].Value;
    bool roleGroup = !string.IsNullOrWhiteSpace(roleParams.Groups[3].Value);
    Role? role = await sender.CreateRoleAsync(new Info(roleName, roleColor, roleGroup));
    if (role != null) await sender.ReplyAsync($"{sender.Author.Tag} 成功创建新角色：\n{role.Name};{role.ColorHtml};{(role.Hoist ? "已" : "未")}分组");
    else await sender.ReplyAsync($"{sender.Author.Tag} 新角色创建失败，{sender.Bot.Info.UserName} 无权执行该操作！");
}, new Regex(@"^(创|新)建角色(\s|\n|$)"), true));
// 指令格式：@机器人 遍历身份组
bot.AddCommand(new Command("统计角色", async (sender, args) =>
{
    Guild? guild = await sender.GetGuildAsync();
    string guildInfo = guild == null ? "" : $"{guild.Name}，成员总数：{guild.MemberCount}\n";
    List<Role>? roles = await sender.GetRolesAsync();
    if (roles == null)
    {
        await sender.ReplyAsync($"{sender.Author.Tag} 获取角色列表失败，{sender.Bot.Info.UserName} 无权执行该操作！");
        return;
    }
    var roleInfoList = roles.Select(r => $"{r.Name}：{r.Number}；{r.ColorHtml}；{(r.Hoist ? "已" : "未") }分组；编号:{r.Id}");
    await sender.ReplyAsync($"{sender.Author.Tag} {guildInfo}共找到 {roleInfoList.Count()} 个身份角色：\n" + string.Join('\n', roleInfoList));
    return;
}, null, true));
// 指令格式：@机器人 修改身份组
bot.AddCommand(new Command("修改角色", async (sender, args) =>
{
    Match m = Regex.Match(args, @"(\S+)\n\s*([^#;\s]+)?;?(#[0-9a-fA-F]+)?;?(分组)?", RegexOptions.RightToLeft);
    string rNameL = m.Groups[1].Value;
    string? rNameR = m.Groups[2].Value;
    if (rNameR.IsBlank()) rNameR = null;
    string? rColor = m.Groups[3].Value;
    if (rColor.IsBlank()) rColor = null;
    bool rGroup = !m.Groups[4].Value.IsBlank();
    if (!m.Success || rNameL.IsBlank())
    {
        await sender.ReplyAsync($"{sender.Author.Tag} 参数不正确！正确格式(使用';'连接参数)：\n￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣\n修改角色 原角色名(或角色ID)\n新角色名(可选);颜色(格式#FFFFFF|可选);分组(可选)");
        return;
    }
    List<Role>? roles = await sender.GetRolesAsync();
    if (roles == null)
    {
        await sender.ReplyAsync($"{sender.Author.Tag} 遍历角色列表失败，{sender.Bot.Info.UserName} 无权执行该操作！");
        return;
    }
    Role? role = roles.Find(x => x.Id == rNameL);
    if (role == null)
    {
        roles.RemoveAll(r => r.Name != rNameL);
        if (!roles.Any()) role = null;
        else if (roles.Count == 1) role = roles[0];
        else
        {
            var roleInfoList = roles.Select(r => $"{r.Name}；{r.ColorHtml}；{(r.Hoist ? "已" : "未") }分组；编号:{r.Id}");
            await sender.ReplyAsync($"{sender.Author.Tag} 找到多个同名角色，请使用角色ID重新发送指令：\n" + string.Join('\n', roleInfoList));
            return;
        }
    }
    if (role == null)
    {
        await sender.ReplyAsync($"{sender.Author.Tag} 在列表中找不到角色：{rNameL}");
        return;
    }
    role = await sender.EditRoleAsync(role.Id, new Info(rNameR, rColor, rGroup));
    if (role != null) await sender.ReplyAsync($"{sender.Author.Tag} 成功修改角色：\n{role.Name}；{role.ColorHtml}；{(role.Hoist ? "已" : "未") }分组；编号:{role.Id}");
    else await sender.ReplyAsync($"{sender.Author.Tag} 修改角色失败，{sender.Bot.Info.UserName} 无权执行该操作！");
}, null, true));
// 指令格式：@机器人 删除身份组
bot.AddCommand(new Command("删除角色", async (sender, args) =>
{
    string rName = args;
    if (rName.IsBlank())
    {
        await sender.ReplyAsync($"{sender.Author.Tag} 参数不正确！正确格式：\n￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣\n删除角色 角色名(或角色ID)");
        return;
    }
    List<Role>? roles = await sender.GetRolesAsync();
    if (roles == null)
    {
        await sender.ReplyAsync($"{sender.Author.Tag} 遍历角色列表失败，{sender.Bot.Info.UserName} 无权执行该操作！");
        return;
    }
    Role? role = roles.Find(x => x.Id == rName);
    if (role == null)
    {
        roles.RemoveAll(r => r.Name != rName);
        if (!roles.Any()) role = null;
        else if (roles.Count == 1) role = roles[0];
        else
        {
            var roleInfoList = roles.Select(r => $"{r.Name}；{r.ColorHtml}；{(r.Hoist ? "已" : "未") }分组；编号:{r.Id}");
            await sender.ReplyAsync($"{sender.Author.Tag} 找到多个同名角色，请使用角色ID重新发送指令：\n" + string.Join('\n', roleInfoList));
            return;
        }
    }
    if (role == null)
    {
        await sender.ReplyAsync($"{sender.Author.Tag} 在列表中找不到角色：{rName}");
        return;
    }
    if (await sender.DeleteRoleAsync(role.Id))
    {
        await sender.ReplyAsync($"{sender.Author.Tag} 成功删除角色：\n{role.Name}；{role.ColorHtml}；{(role.Hoist ? "已" : "未") }分组；编号:{role.Id}");
    }
    else await sender.ReplyAsync($"{sender.Author.Tag} 删除角色失败，{sender.Bot.Info.UserName} 无权执行该操作！");
}, null, true));
// 指令格式：@机器人 增加身份组成员 @用户 @用户2
bot.AddCommand(new Command("增加角色成员", async (sender, args) =>
{
    string rName = Regex.Replace(args, @"<@!\d+>", "").Trim();
    List<User>? users = sender.Mentions;
    if (!args.Contains(sender.Bot.Info.Tag)) users?.RemoveAll(user => user.Id == sender.Bot.Info.Id); // 排除机器人自己
    users = users?.ToHashSet().ToList();
    if (rName.IsBlank() || users?.Any() != true)
    {
        await sender.ReplyAsync($"{sender.Author.Tag} 参数不正确！正确格式：\n￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣\n增加角色成员 角色名(或角色ID) @用户(可同时添加多个)");
        return;
    }
    List<Role>? roles = await sender.GetRolesAsync();
    if (roles == null)
    {
        await sender.ReplyAsync($"{sender.Author.Tag} 遍历角色列表失败，{sender.Bot.Info.UserName} 无权执行该操作！");
        return;
    }
    Role? role = roles.Find(x => x.Id == rName);
    if (role == null)
    {
        roles.RemoveAll(r => r.Name != rName);
        if (!roles.Any()) role = null;
        else if (roles.Count == 1) role = roles[0];
        else
        {
            var roleInfoList = roles.Select(r => $"{r.Name}；{r.ColorHtml}；{(r.Hoist ? "已" : "未") }分组；编号:{r.Id}");
            await sender.ReplyAsync($"{sender.Author.Tag} 找到多个同名角色，请使用角色ID重新发送指令：\n" + string.Join('\n', roleInfoList));
            return;
        }
    }
    if (role == null)
    {
        await sender.ReplyAsync($"{sender.Author.Tag} 在列表中找不到角色：{rName}");
        return;
    }
    foreach (var user in users)
    {
        bool isOk = await sender.AddRoleMemberAsync(user, role.Id, role.Id == "5" ? sender.ChannelId : null);
        string replyMsg = isOk ? "成功" : $"失败，{sender.Bot.Info.UserName} 无权执行该操作！";
        await sender.ReplyAsync($"{user.Tag} 加入角色组 {role.Name} {replyMsg}");
    }
}, null, true));
// 指令格式：@机器人 删除身份组成员 @用户 @用户2
bot.AddCommand(new Command("删除角色成员", async (sender, args) =>
{
    string rName = Regex.Replace(args, @"<@!\d+>", "").Trim();
    List<User>? users = sender.Mentions;
    if (!args.Contains(sender.Bot.Info.Tag)) users?.RemoveAll(user => user.Id == sender.Bot.Info.Id); // 排除机器人自己
    users = users?.ToHashSet().ToList();
    if (rName.IsBlank() || users?.Any() != true)
    {
        await sender.ReplyAsync($"{sender.Author.Tag} 参数不正确！正确格式：\n￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣￣\n删除角色成员 角色名(或角色ID) @用户(可同时添加多个)");
        return;
    }
    List<Role>? roles = await sender.GetRolesAsync();
    if (roles == null)
    {
        await sender.ReplyAsync($"{sender.Author.Tag} 遍历角色列表失败，{sender.Bot.Info.UserName} 无权执行该操作！");
        return;
    }
    Role? role = roles.Find(x => x.Id == rName);
    if (role == null)
    {
        roles.RemoveAll(r => r.Name != rName);
        if (!roles.Any()) role = null;
        else if (roles.Count == 1) role = roles[0];
        else
        {
            var roleInfoList = roles.Select(r => $"{r.Name}；{r.ColorHtml}；{(r.Hoist ? "已" : "未") }分组；编号:{r.Id}");
            await sender.ReplyAsync($"{sender.Author.Tag} 找到多个同名角色，请使用角色ID重新发送指令：\n" + string.Join('\n', roleInfoList));
            return;
        }
    }
    if (role == null)
    {
        await sender.ReplyAsync($"{sender.Author.Tag} 在列表中找不到角色：{rName}");
        return;
    }
    foreach (var user in users)
    {
        bool isOk = await sender.DeleteRoleMemberAsync(user, role.Id, role.Id == "5" ? sender.ChannelId : null);
        string replyMsg = isOk ? "成功" : $"失败，{sender.Bot.Info.UserName} 无权执行该操作！";
        await sender.ReplyAsync($"{user.Tag} 移出角色组 {role.Name} {replyMsg}");
    }
}, null, true));
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
