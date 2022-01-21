﻿using QQChannelBot.Models;
using QQChannelBot.MsgHelper;

namespace QQChannelBot.Bot
{
    /// <summary>
    /// 发件人对象
    /// </summary>
    public class Sender
    {
        /// <summary>
        /// Sender构造器
        /// </summary>
        /// <param name="message">收到的消息对象</param>
        /// <param name="bot">收到消息的机器人</param>
        public Sender(Message message, BotClient bot)
        {
            this.Message = message;
            Bot = bot;
            ReportError = bot.ReportApiError;
        }
        /// <summary>
        /// 收到消息的机器人
        /// </summary>
        public BotClient Bot { get; init; }
        /// <summary>
        /// 收到的消息对象
        /// <para>若没有执行对象，可为空</para>
        /// </summary>
        public Message Message { get; init; }
        /// <summary>
        /// 消息类型
        /// </summary>
        public MessageType MessageType { get; set; } = MessageType.Public;
        /// <summary>
        /// 发件人的消息内容
        /// </summary>
        public string Content => this.Message.Content;
        /// <summary>
        /// 发件人的用户信息
        /// </summary>
        public User Author => this.Message.Author;
        /// <summary>
        /// 发件人的成员信息
        /// </summary>
        public Member Member => this.Message.Member;
        /// <summary>
        /// 发件人提到的用户
        /// </summary>
        public List<User>? Mentions => this.Message.Mentions;
        /// <summary>
        /// 发件人所在的频道
        /// </summary>
        public string GuildId => this.Message.GuildId;
        /// <summary>
        /// 发件人所在的子频道
        /// </summary>
        public string ChannelId => this.Message.ChannelId;
        /// <summary>
        /// 向发件人报告API调用失败的错误信息
        /// <para>默认值为 <see cref="BotClient.ReportApiError"/></para>
        /// </summary>
        public bool ReportError { get; set; }
        /// <summary>
        /// 回复发件人
        /// <para>自动填充被动消息参数</para>
        /// </summary>
        /// <param name="message">MessageToCreate消息构造对象(或其扩展对象)</param>
        /// <returns></returns>
        public async Task<Message?> ReplyAsync(MessageToCreate message)
        {
            message.Id = this.Message.Id;
            return await (this.MessageType == MessageType.Private ? Bot.SendPMAsync(this.GuildId, message) : Bot.SendMessageAsync(this.ChannelId, message, this));
        }
        /// <summary>
        /// 回复发件人
        /// <para>自动填充被动消息参数</para>
        /// </summary>
        /// <param name="message">文字消息内容</param>
        /// <returns></returns>
        public async Task<Message?> ReplyAsync(string message) => await ReplyAsync(new MsgText(message));

        #region 简化API调用
        /// <summary>
        /// 获取当前用户(机器人)信息
        /// <para>此API无需任何权限</para>
        /// </summary>
        /// <returns></returns>
        public async Task<User?> GetMeAsync() => await Bot.GetMeAsync(this);
        /// <summary>
        /// 获取当前用户(机器人)已加入频道列表
        /// <para>此API无需任何权限</para>
        /// </summary>
        /// <param name="guild_id">频道Id（作为拉取下一次列表的分页坐标使用）</param>
        /// <param name="route">数据拉取方向（true-向前查找 | false-向后查找）</param>
        /// <param name="limit">数据分页（默认每次拉取100条）</param>
        /// <returns></returns>
        public async Task<List<Guild>?> GetMeGuildsAsync(string? guild_id = null, bool route = false, int limit = 100) => await Bot.GetMeGuildsAsync(guild_id, route, limit, this);
        /// <summary>
        /// 获取频道详情
        /// </summary>
        /// <returns>Guild?</returns>
        public async Task<Guild?> GetGuildAsync() => await Bot.GetGuildAsync(this.Message.GuildId, this);
        /// <summary>
        /// 获取子频道列表
        /// <para>目标子频道为发件人当前子频道</para>
        /// </summary>
        /// <param name="type">筛选子频道类型</param>
        /// <param name="subType">筛选子频道子类型</param>
        /// <returns></returns>
        public async Task<List<Channel>?> GetChannelsAsync(ChannelType? type = null, ChannelSubType? subType = null) => await Bot.GetChannelsAsync(this.GuildId, type, subType, this);
        /// <summary>
        /// 获取子频道详情
        /// <para>目标子频道为发件人当前子频道</para>
        /// </summary>
        /// <returns></returns>
        public async Task<Channel?> GetChannelAsync() => await Bot.GetChannelAsync(this.ChannelId, this);
        /// <summary>
        /// 创建子频道（仅私域可用）
        /// <para>
        /// 以下字段必填：<br/>
        /// Channel.Name - 子频道名称<br/>
        /// Channel.Type - 子频道类型<br/>
        /// </para>
        /// </summary>
        /// <param name="channel">用于创建子频道的对象</param>
        /// <returns></returns>
        public async Task<Channel?> CreateChannelAsync(Channel channel)
        {
            if (channel.GuildId.IsBlank()) channel.GuildId = this.GuildId;
            return await Bot.CreateChannelAsync(channel, this);
        }
        /// <summary>
        /// 修改子频道（仅私域可用）
        /// </summary>
        /// <param name="channel">修改属性后的子频道对象</param>
        /// <returns></returns>
        public async Task<Channel?> EditChannelAsync(Channel channel) => await Bot.EditChannelAsync(channel, this);
        /// <summary>
        /// 删除子频道（仅私域可用）
        /// </summary>
        /// <param name="channel">指定要删除的子频道</param>
        /// <returns></returns>
        public async Task<bool> DeleteChannelAsync(Channel channel) => await Bot.DeleteChannelAsync(channel.Id, this);
        /// <summary>
        /// 获取频道成员列表（仅私域可用）
        /// <para>目标频道为发件人当前频道</para>
        /// </summary>
        /// <param name="limit">分页大小1-1000（默认值100）</param>
        /// <param name="after">上次回包中最后一个Member的用户ID，首次请求填"0"</param>
        /// <returns></returns>
        public async Task<List<Member>?> GetGuildMembersAsync(int limit = 100, string? after = null) => await Bot.GetGuildMembersAsync(this.GuildId, limit, after, this);
        /// <summary>
        /// 获取频道成员详情
        /// <para>目标频道为发件人当前频道</para>
        /// </summary>
        /// <param name="user">成员用户对象</param>
        /// <returns></returns>
        public async Task<Member?> GetMemberAsync(User user) => await Bot.GetMemberAsync(this.GuildId, user.Id, this);
        /// <summary>
        /// 删除频道成员（仅私域可用）
        /// <para>目标频道为发件人当前频道</para>
        /// </summary>
        /// <param name="user">成员用户对象</param>
        /// <returns></returns>
        public async Task<bool> DeleteGuildMemberAsync(User user) => await Bot.DeleteGuildMemberAsync(this.GuildId, user.Id, this);
        /// <summary>
        /// 获取频道身份组列表
        /// </summary>
        public async Task<List<Role>?> GetRolesAsync() => await Bot.GetRolesAsync(this.Message.GuildId, this);
        /// <summary>
        /// 创建频道身份组
        /// </summary>
        /// <param name="info">携带需要设置的字段内容</param>
        /// <returns></returns>
        public async Task<Role?> CreateRoleAsync(Info info) => await Bot.CreateRoleAsync(this.GuildId, info, null, this);
        /// <summary>
        /// 修改频道身份组
        /// </summary>
        /// <param name="role_id">角色Id</param>
        /// <param name="info">携带需要修改的字段内容</param>
        /// <returns></returns>
        public async Task<Role?> EditRoleAsync(string role_id, Info info) => await Bot.EditRoleAsync(this.GuildId, role_id, info, null, this);
        /// <summary>
        /// 删除频道身份组
        /// </summary>
        /// <param name="role_id">角色Id</param>
        /// <returns></returns>
        public async Task<bool> DeleteRoleAsync(string role_id) => await Bot.DeleteRoleAsync(this.GuildId, role_id, this);
        /// <summary>
        /// 创建频道身份组成员
        /// <para>
        /// 如果要增加的身份组ID是(5-子频道管理员)，需要设置 channel_id 来指定目标子频道。
        /// </para>
        /// </summary>
        /// <param name="user">要加入身份组的用户</param>
        /// <param name="role_id">身份组Id</param>
        /// <param name="channel_id">子频道Id</param>
        /// <returns></returns>
        public async Task<bool> AddRoleMemberAsync(User user, string role_id, string? channel_id = null) => await Bot.AddRoleMemberAsync(this.GuildId, user.Id, role_id, channel_id, this);
        /// <summary>
        /// 删除频道身份组成员
        /// <para>
        /// 如果要移除的身份组ID是(5-子频道管理员)，需要设置 channel_id 来指定目标子频道。
        /// </para>
        /// </summary>
        /// <param name="user">要移除身份组的用户Id</param>
        /// <param name="role_id">身份组Id</param>
        /// <param name="channel_id">子频道Id</param>
        /// <returns></returns>
        public async Task<bool> DeleteRoleMemberAsync(User user, string role_id, string? channel_id = null) => await Bot.DeleteRoleMemberAsync(this.GuildId, user.Id, role_id, channel_id, this);
        /// <summary>
        /// 获取子频道权限
        /// </summary>
        /// <param name="user">目标用户（默认为发件人）</param>
        /// <param name="channel_id">目标子频道Id（默认为发件人当前频道）</param>
        /// <returns></returns>
        public async Task<ChannelPermissions?> GetChannelPermissionsAsync(User? user = null, string? channel_id = null) => await Bot.GetChannelPermissionsAsync(channel_id ?? this.ChannelId, user?.Id ?? this.Author.Id, this);
        /// <summary>
        /// 修改子频道权限
        /// </summary>
        /// <param name="permission">修改后的权限</param>
        /// <param name="user">目标用户</param>
        /// <param name="channel_id">目标子频道Id（默认为发件人当前子频道）</param>
        /// <returns></returns>
        public async Task<bool> EditChannelPermissionsAsync(PrivacyType permission, User user, string? channel_id = null) => await Bot.EditChannelPermissionsAsync(channel_id ?? this.ChannelId, user.Id, permission, this);
        /// <summary>
        /// 获取子频道身份组权限
        /// </summary>
        /// <param name="role_id">目标身份组Id</param>
        /// <param name="channel_id">目标子频道Id（默认为发件人当前子频道）</param>
        /// <returns></returns>
        public async Task<ChannelPermissions?> GetMemberChannelPermissionsAsync(string role_id, string? channel_id = null) => await Bot.GetMemberChannelPermissionsAsync(channel_id ?? this.ChannelId, role_id, this);
        /// <summary>
        /// 修改子频道身份组权限
        /// <para>注：本接口不支持修改 "可管理子频道" 权限</para>
        /// </summary>
        /// <param name="permission">修改后的权限</param>
        /// <param name="role_id">目标身份组Id</param>
        /// <param name="channel_id">目标子频道Id（默认为发件人当前子频道）</param>
        /// <returns></returns>
        public async Task<bool> EditMemberChannelPermissionsAsync(PrivacyType permission, string role_id, string? channel_id = null) => await Bot.EditMemberChannelPermissionsAsync(channel_id ?? this.ChannelId, role_id, permission, this);

        /// <summary>
        /// 获取消息列表（待验证）
        /// </summary>
        /// <param name="message">作为坐标的消息（需要消息Id和子频道Id）
        /// <para>默认使用发件人消息</para>
        /// </param>
        /// <param name="limit">分页大小（1-20）</param>
        /// <param name="typesEnum">拉取类型（默认拉取最新消息）</param>
        /// <returns></returns>
        public async Task<List<Message>?> GetMessagesAsync(Message? message = null, int limit = 20, GetMsgTypesEnum? typesEnum = null) => await Bot.GetMessagesAsync(message ?? this.Message, limit, typesEnum, this);
        /// <summary>
        /// 获取指定消息
        /// </summary>
        /// <param name="message">目标消息对象（可使用消息id和子频道id构造）</param>
        /// <returns></returns>
        public async Task<Message?> GetMessageAsync(Message message) => await Bot.GetMessageAsync(message.ChannelId, message.Id, this);
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">准备要发送的消息对象</param>
        /// <param name="channel_id">目标子频道Id（默认为发件人当前子频道）</param>
        /// <returns></returns>
        public async Task<Message?> SendMessageAsync(MessageToCreate message, string? channel_id = null) => await Bot.SendMessageAsync(channel_id ?? this.ChannelId, message, this);
        /// <summary>
        /// 撤回消息
        /// </summary>
        /// <param name="message">目标消息对象</param>
        /// <returns></returns>
        public async Task<bool> DeleteMessageAsync(Message message) => await Bot.DeleteMessageAsync(message.ChannelId, message.Id, this);
        /// <summary>
        /// 撤回机器人在当前子频道发出的最后一条消息
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DeleteLastMessageAsync() => await Bot.DeleteLastMessageAsync(this.Message, this);
        /// <summary>
        /// 创建私信会话
        /// </summary>
        /// <param name="user">私信用户对象</param>
        /// <returns></returns>
        public async Task<DirectMessageSource?> CreateDMSAsync(User user) => await Bot.CreateDMSAsync(user.Id, this.GuildId, this);
        /// <summary>
        /// 发送私信
        /// </summary>
        /// <param name="message">准备要发送的消息对象</param>
        /// <param name="guild_id">私信会话频道Id</param>
        /// <returns></returns>
        public async Task<Message?> SendPMAsync(MessageToCreate message, string? guild_id = null) => await Bot.SendPMAsync(guild_id ?? this.GuildId, message, this);

        /// <summary>
        /// 禁言全员
        /// </summary>
        /// <param name="muteTime"></param>
        /// <returns></returns>
        public async Task<bool> MuteGuildAsync(MuteTime muteTime) => await Bot.MuteGuildAsync(this.GuildId, muteTime, this);
        /// <summary>
        /// 禁言指定成员
        /// </summary>
        /// <param name="user">目标成员用户</param>
        /// <param name="muteTime">禁言时间</param>
        /// <returns></returns>
        public async Task<bool> MuteMemberAsync(User user, MuteTime muteTime) => await Bot.MuteMemberAsync(this.GuildId, user.Id, muteTime, this);
        /// <summary>
        /// 创建频道全局公告
        /// <para>默认目标为发件人当前频道</para>
        /// </summary>
        /// <param name="message">作为公告的消息</param>
        /// <returns></returns>
        public async Task<Announces?> CreateAnnouncesGlobalAsync(Message message) => await Bot.CreateAnnouncesGlobalAsync(this.GuildId, this.ChannelId, message.Id, this);
        /// <summary>
        /// 删除频道全局公告
        /// <para>默认目标为发件人当前频道</para>
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DeleteAnnouncesGlobalAsync() => await Bot.DeleteAnnouncesGlobalAsync(this.GuildId, "all", this);
        /// <summary>
        /// 创建子频道公告
        /// <para>默认目标为发件人当前子频道</para>
        /// </summary>
        /// <param name="message">作为公告的消息</param>
        /// <param name="channel_id">公告的子频道Id（默认使用公告消息发送的子频道）</param>
        /// <returns></returns>
        public async Task<Announces?> CreateAnnouncesAsync(Message message, string? channel_id = null) => await Bot.CreateAnnouncesAsync(channel_id ?? message.ChannelId, message.Id, this);
        /// <summary>
        /// 删除子频道公告
        /// <para>默认目标为发件人当前子频道</para>
        /// </summary>
        /// <param name="channel_id">公告的子频道Id（默认使用发件人当前所在的子频道）</param>
        /// <returns></returns>
        public async Task<bool> DeleteAnnouncesAsync(string? channel_id = null) => await Bot.DeleteAnnouncesAsync(channel_id ?? this.ChannelId, "all", this);
        /// <summary>
        /// 获取日程列表
        /// </summary>
        /// <param name="channel_id">日程子频道Id</param>
        /// <param name="since">筛选日程开始时间（默认为当日全天）</param>
        /// <returns></returns>
        public async Task<List<Schedule>?> GetSchedulesAsync(string channel_id, DateTime? since = null) => await Bot.GetSchedulesAsync(channel_id, since, this);
        /// <summary>
        /// 获取日程详情
        /// </summary>
        /// <param name="channel_id">日程子频道Id</param>
        /// <param name="schedule_id">日程Id</param>
        /// <returns></returns>
        public async Task<Schedule?> GetScheduleAsync(string channel_id, string schedule_id) => await Bot.GetScheduleAsync(channel_id, schedule_id, this);
        /// <summary>
        /// 创建日程
        /// <para>
        /// 日程开始时间必须大于当前时间。
        /// </para>
        /// </summary>
        /// <param name="channel_id">日程子频道Id</param>
        /// <param name="schedule">新的日程对象，不需要带Id</param>
        /// <returns></returns>
        public async Task<Schedule?> CreateScheduleAsync(string channel_id, Schedule schedule) => await Bot.CreateScheduleAsync(channel_id, schedule, this);
        /// <summary>
        /// 修改日程
        /// </summary>
        /// <param name="channel_id">日程子频道Id</param>
        /// <param name="schedule">修改后的日程对象</param>
        /// <returns></returns>
        public async Task<Schedule?> EditScheduleAsync(string channel_id, Schedule schedule) => await Bot.EditScheduleAsync(channel_id, schedule, this);
        /// <summary>
        /// 删除日程
        /// </summary>
        /// <param name="channel_id">日程子频道Id</param>
        /// <param name="schedule">日程对象（主要需要Id）</param>
        /// <returns></returns>
        public async Task<bool> DeleteScheduleAsync(string channel_id, Schedule schedule) => await Bot.DeleteScheduleAsync(channel_id, schedule, this);
        /// <summary>
        /// 音频控制
        /// </summary>
        /// <param name="audioControl">音频对象</param>
        /// <param name="channel_id">子频道Id（默认为发件人当前子频道）</param>
        /// <returns></returns>
        public async Task<Message?> AudioControlAsync(AudioControl audioControl, string? channel_id = null) => await Bot.AudioControlAsync(channel_id ?? this.ChannelId, audioControl, this);
        #endregion
    }
}
