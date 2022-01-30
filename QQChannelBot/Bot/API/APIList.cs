namespace QQChannelBot.Bot
{
    /// <summary>
    /// API类型
    /// </summary>
    public enum APIType
    {
        /// <summary>
        /// 任何机器人可以直接使用
        /// </summary>
        基础,
        /// <summary>
        /// 公域机器人获得频道主授权后可用
        /// <para>注：私域包含公域</para>
        /// </summary>
        公域,
        /// <summary>
        /// 私域机器人获得频道主授权后可用
        /// </summary>
        私域
    }
    /// <summary>
    /// API信息结构体
    /// </summary>
    /// <param name="Type">类型</param>
    /// <param name="Method">请求方式</param>
    /// <param name="Path">接口地址</param>
    public record struct BotAPI(APIType Type, HttpMethod Method, string Path);
    /// <summary>
    /// 机器人API列表
    /// </summary>
    public record struct APIList
    {
        /// <summary>
        /// 获取当前用户（机器人）详情
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/user/me.html">接口文档</see><br/>
        /// 无需鉴权<br/>
        /// GET /users/@me
        /// </para>
        /// </summary>
        public static BotAPI 获取用户详情 => new(APIType.基础, HttpMethod.Get, @"/users/@me");
        /// <summary>
        /// 获取当前用户（机器人）所加入的频道列表
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/user/guilds.html">接口文档</see><br/>
        /// 无需鉴权<br/>
        /// GET /users/@me/guilds
        /// </para>
        /// </summary>
        public static BotAPI 获取用户频道列表 => new(APIType.基础, HttpMethod.Get, @"/users/@me/guilds");

        /// <summary>
        /// 获取 guild_id 指定的频道的详情
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/guild/get_guild.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// GET /guilds/{guild_id}
        /// </para>
        /// </summary>
        public static BotAPI 获取频道详情 => new(APIType.公域, HttpMethod.Get, @"/guilds/{guild_id}");

        /// <summary>
        /// 获取 guild_id 指定的频道下的子频道列表
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/channel/get_channels.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// GET /guilds/{guild_id}/channels
        /// </para>
        /// </summary>
        public static BotAPI 获取子频道列表 => new(APIType.公域, HttpMethod.Get, @"/guilds/{guild_id}/channels");
        /// <summary>
        /// 获取 channel_id 指定的子频道的详情
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/channel/get_channel.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// GET /channels/{channel_id}
        /// </para>
        /// </summary>
        public static BotAPI 获取子频道详情 => new(APIType.公域, HttpMethod.Get, @"/channels/{channel_id}");
        /// <summary>
        /// 在 guild_id 指定的频道下创建一个子频道
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/channel/post_channels.html">接口文档</see><br/>
        /// 私域鉴权<br/>
        /// POST /guilds/{guild_id}/channels
        /// </para>
        /// </summary>
        public static BotAPI 创建子频道 => new(APIType.私域, HttpMethod.Post, @"/guilds/{guild_id}/channels");
        /// <summary>
        /// 修改 channel_id 指定的子频道的信息
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/channel/patch_channel.html">接口文档</see><br/>
        /// 私域鉴权<br/>
        /// PATCH /channels/{channel_id}
        /// </para>
        /// </summary>
        public static BotAPI 修改子频道 => new(APIType.私域, HttpMethod.Patch, @"/channels/{channel_id}");
        /// <summary>
        /// 删除 channel_id 指定的子频道
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/channel/delete_channel.html">接口文档</see><br/>
        /// 私域鉴权<br/>
        /// DELETE /channels/{channel_id}
        /// </para>
        /// </summary>
        public static BotAPI 删除子频道 => new(APIType.私域, HttpMethod.Delete, @"/channels/{channel_id}");

        /// <summary>
        /// 获取 guild_id 指定的频道中所有成员的详情列表
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/member/get_members.html">接口文档</see><br/>
        /// 私域鉴权<br/>
        /// GET /guilds/{guild_id}/members
        /// </para>
        /// </summary>
        public static BotAPI 获取频道成员列表 => new(APIType.私域, HttpMethod.Get, @"/guilds/{guild_id}/members");
        /// <summary>
        /// 获取 guild_id 指定的频道中 user_id 对应成员的详细信息
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/member/get_member.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// GET /guilds/{guild_id}/members/{user_id}
        /// </para>
        /// </summary>
        public static BotAPI 获取成员详情 => new(APIType.公域, HttpMethod.Get, @"/guilds/{guild_id}/members/{user_id}");
        /// <summary>
        /// 删除 guild_id 指定的频道中 user_id 对应的成员
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/member/delete_member.html">接口文档</see><br/>
        /// 私域鉴权<br/>
        /// DELETE /guilds/{guild_id}/members/{user_id}
        /// </para>
        /// </summary>
        public static BotAPI 删除频道成员 => new(APIType.私域, HttpMethod.Delete, @"/guilds/{guild_id}/members/{user_id}");

        /// <summary>
        /// 获取 guild_id 指定的频道下的身份组列表
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/guild/get_guild_roles.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// GET /guilds/{guild_id}/roles
        /// </para>
        /// </summary>
        public static BotAPI 获取频道身份组列表 => new(APIType.公域, HttpMethod.Get, @"/guilds/{guild_id}/roles");
        /// <summary>
        /// 在 guild_id 指定的频道下创建一个身份组
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/guild/post_guild_role.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// POST /guilds/{guild_id}/roles
        /// </para>
        /// </summary>
        public static BotAPI 创建频道身份组 => new(APIType.公域, HttpMethod.Post, @"/guilds/{guild_id}/roles");
        /// <summary>
        /// 修改频道 guild_id 下 role_id 指定的身份组
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/guild/patch_guild_role.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// PATCH /guilds/{guild_id}/roles/{role_id}
        /// </para>
        /// </summary>
        public static BotAPI 修改频道身份组 => new(APIType.公域, HttpMethod.Patch, @"/guilds/{guild_id}/roles/{role_id}");
        /// <summary>
        /// 删除频道 guild_id 下 role_id 指定的身份组
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/guild/delete_guild_role.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// DELETE /guilds/{guild_id}/roles/{role_id}
        /// </para>
        /// </summary>
        public static BotAPI 删除频道身份组 => new(APIType.公域, HttpMethod.Delete, @"/guilds/{guild_id}/roles/{role_id}");
        /// <summary>
        /// 将频道 guild_id 下的用户 user_id 添加到身份组 role_id
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/guild/put_guild_member_role.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// PUT /guilds/{guild_id}/members/{user_id}/roles/{role_id}
        /// </para>
        /// </summary>
        public static BotAPI 添加频道身份组成员 => new(APIType.公域, HttpMethod.Put, @"/guilds/{guild_id}/members/{user_id}/roles/{role_id}");
        /// <summary>
        /// 将用户 user_id 从频道 guild_id 的 role_id 身份组中移除
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/guild/delete_guild_member_role.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// DELETE /guilds/{guild_id}/members/{user_id}/roles/{role_id}
        /// </para>
        /// </summary>
        public static BotAPI 删除频道身份组成员 => new(APIType.公域, HttpMethod.Delete, @"/guilds/{guild_id}/members/{user_id}/roles/{role_id}");

        /// <summary>
        /// 获取子频道 channel_id 下用户 user_id 的权限
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/channel_permissions/get_channel_permissions.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// GET /channels/{channel_id}/members/{user_id}/permissions
        /// </para>
        /// </summary>
        public static BotAPI 获取子频道用户权限 => new(APIType.公域, HttpMethod.Get, @"/channels/{channel_id}/members/{user_id}/permissions");
        /// <summary>
        /// 修改子频道 channel_id 下用户 user_id 的权限
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/channel_permissions/put_channel_permissions.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// PUT /channels/{channel_id}/members/{user_id}/permissions
        /// </para>
        /// </summary>
        public static BotAPI 修改子频道用户权限 => new(APIType.公域, HttpMethod.Put, @"/channels/{channel_id}/members/{user_id}/permissions");
        /// <summary>
        /// 获取子频道 channel_id 下身份组 role_id 的权限
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/channel_permissions/get_channel_roles_permissions.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// GET /channels/{channel_id}/roles/{role_id}/permissions
        /// </para>
        /// </summary>
        public static BotAPI 获取子频道身份组权限 => new(APIType.公域, HttpMethod.Get, @"/channels/{channel_id}/roles/{role_id}/permissions");
        /// <summary>
        /// 修改子频道 channel_id 下身份组 role_id 的权限
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/channel_permissions/put_channel_roles_permissions.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// PUT /channels/{channel_id}/roles/{role_id}/permissions
        /// </para>
        /// </summary>
        public static BotAPI 修改子频道身份组权限 => new(APIType.公域, HttpMethod.Put, @"/channels/{channel_id}/roles/{role_id}/permissions");

        /// <summary>
        /// 获取子频道 channel_id 下的消息 message_id 的详情
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/message/get_message_of_id.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// GET /channels/{channel_id}/messages/{message_id}
        /// </para>
        /// </summary>
        public static BotAPI 获取指定消息 => new(APIType.公域, HttpMethod.Get, @"/channels/{channel_id}/messages/{message_id}");
        /// <summary>
        /// 获取子频道 channel_id 下的消息列表
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/pythonsdk/api/message/get_messages.html">接口文档</see><br/>
        /// 私域鉴权<br/>
        /// GET /channels/{channel_id}/messages
        /// </para>
        /// </summary>
        public static BotAPI 获取消息列表 => new(APIType.私域, HttpMethod.Get, @"/channels/{channel_id}/messages");
        /// <summary>
        /// 向 channel_id 指定的子频道发送消息
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/message/post_messages.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// POST /channels/{channel_id}/messages
        /// </para>
        /// </summary>
        public static BotAPI 发送消息 => new(APIType.公域, HttpMethod.Post, @"/channels/{channel_id}/messages");
        /// <summary>
        /// 撤回 message_id 指定的消息
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/nodesdk/message/delete_message.html">接口文档</see><br/>
        /// 私域鉴权<br/>
        /// DELETE /channels/{channel_id}/messages/{message_id}
        /// </para>
        /// </summary>
        public static BotAPI 撤回消息 => new(APIType.私域, HttpMethod.Delete, @"/channels/{channel_id}/messages/{message_id}");

        /// <summary>
        /// 机器人和在同一个频道内的成员创建私信会话
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/dms/post_dms.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// POST /users/@me/dms
        /// </para>
        /// </summary>
        public static BotAPI 创建私信会话 => new(APIType.公域, HttpMethod.Post, @"/users/@me/dms");
        /// <summary>
        /// 发送私信消息（已经创建私信会话后）
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/dms/post_dms_messages.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// POST /dms/{guild_id}/messages
        /// </para>
        /// </summary>
        public static BotAPI 发送私信 => new(APIType.公域, HttpMethod.Post, @"/dms/{guild_id}/messages");

        /// <summary>
        /// 将频道的全体成员（非管理员）禁言
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/guild/patch_guild_mute.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// PATCH /guilds/{guild_id}/mute
        /// </para>
        /// </summary>
        public static BotAPI 禁言全员 => new(APIType.公域, HttpMethod.Patch, @"/guilds/{guild_id}/mute");
        /// <summary>
        /// 禁言频道 guild_id 下的成员 user_id
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/guild/patch_guild_member_mute.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// PATCH /guilds/{guild_id}/members/{user_id}/mute
        /// </para>
        /// </summary>
        public static BotAPI 禁言指定成员 => new(APIType.公域, HttpMethod.Patch, @"/guilds/{guild_id}/members/{user_id}/mute");

        /// <summary>
        /// 将频道 guild_id 内的某条消息设置为频道全局公告
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/announces/post_guild_announces.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// POST /guilds/{guild_id}/announces
        /// </para>
        /// </summary>
        public static BotAPI 创建频道公告 => new(APIType.公域, HttpMethod.Post, @"/guilds/{guild_id}/announces");
        /// <summary>
        /// 删除频道 guild_id 下 message_id 指定的全局公告
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/announces/delete_guild_announces.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// DELETE /guilds/{guild_id}/announces/{message_id}
        /// </para>
        /// </summary>
        public static BotAPI 删除频道公告 => new(APIType.公域, HttpMethod.Delete, @"/guilds/{guild_id}/announces/{message_id}");
        /// <summary>
        /// 将子频道 channel_id 内的某条消息设置为子频道公告
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/announces/post_channel_announces.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// POST /channels/{channel_id}/announces
        /// </para>
        /// </summary>
        public static BotAPI 创建子频道公告 => new(APIType.公域, HttpMethod.Post, @"/channels/{channel_id}/announces");
        /// <summary>
        /// 删除子频道 channel_id 下 message_id 指定的子频道公告
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/announces/delete_channel_announces.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// DELETE /channels/{channel_id}/announces/{message_id}
        /// </para>
        /// </summary>
        public static BotAPI 删除子频道公告 => new(APIType.公域, HttpMethod.Delete, @"/channels/{channel_id}/announces/{message_id}");

        /// <summary>
        /// 获取 channel_id 指定的子频道中当天的日程列表
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/schedule/get_schedules.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// GET /channels/{channel_id}/schedules
        /// </para>
        /// </summary>
        public static BotAPI 获取频道日程列表 => new(APIType.公域, HttpMethod.Get, @"/channels/{channel_id}/schedules");
        /// <summary>
        /// 获取日程子频道 channel_id 下 schedule_id 指定的的日程的详情
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/schedule/get_schedule.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// GET /channels/{channel_id}/schedules/{schedule_id}
        /// </para>
        /// </summary>
        public static BotAPI 获取日程详情 => new(APIType.公域, HttpMethod.Get, @"/channels/{channel_id}/schedules/{schedule_id}");
        /// <summary>
        /// 在 channel_id 指定的日程子频道下创建一个日程
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/schedule/post_schedule.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// POST /channels/{channel_id}/schedules
        /// </para>
        /// </summary>
        public static BotAPI 创建日程 => new(APIType.公域, HttpMethod.Post, @"/channels/{channel_id}/schedules");
        /// <summary>
        /// 修改日程子频道 channel_id 下 schedule_id 指定的日程的详情
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/schedule/patch_schedule.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// PATCH /channels/{channel_id}/schedules/{schedule_id}
        /// </para>
        /// </summary>
        public static BotAPI 修改日程 => new(APIType.公域, HttpMethod.Patch, @"/channels/{channel_id}/schedules/{schedule_id}");
        /// <summary>
        /// 删除日程子频道 channel_id 下 schedule_id 指定的日程
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/schedule/delete_schedule.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// DELETE /channels/{channel_id}/schedules/{schedule_id}
        /// </para>
        /// </summary>
        public static BotAPI 删除日程 => new(APIType.公域, HttpMethod.Delete, @"/channels/{channel_id}/schedules/{schedule_id}");

        /// <summary>
        /// 控制子频道 channel_id 下的音频
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/audio/audio_control.html">接口文档</see><br/>
        /// 公域鉴权<br/>
        /// POST /channels/{channel_id}/audio
        /// </para>
        /// </summary>
        public static BotAPI 音频控制 => new(APIType.公域, HttpMethod.Post, @"/channels/{channel_id}/audio");

        /// <summary>
        /// 获取机器人在频道 guild_id 内可以使用的权限列表
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/api_permissions/get_guild_api_permission.html">接口文档</see><br/>
        /// 无需鉴权<br/>
        /// GET /guilds/{guild_id}/api_permission
        /// </para>
        /// </summary>
        public static BotAPI 获取频道可用权限列表 => new(APIType.基础, HttpMethod.Get, @"/guilds/{guild_id}/api_permission");
        /// <summary>
        /// 创建 API 接口权限授权链接，该链接指向 guild_id 对应的频道
        /// <para>
        /// <see href="https://bot.q.qq.com/wiki/develop/api/openapi/api_permissions/post_api_permission_demand.html">接口文档</see><br/>
        /// 无需鉴权<br/>
        /// POST /guilds/{guild_id}/api_permission/demand
        /// </para>
        /// </summary>
        public static BotAPI 创建频道接口授权链接 => new(APIType.基础, HttpMethod.Post, @"/guilds/{guild_id}/api_permission/demand");
    }
}