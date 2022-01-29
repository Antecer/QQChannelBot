﻿using System.Text.Json.Serialization;

namespace QQChannelBot.Bot.StatusCode
{
    /// <summary>
    /// 状态码对照表
    /// </summary>
    public static class StatusCodes
    {
        /// <summary>
        /// API请求状态码对照表
        /// </summary>
        public static readonly Dictionary<int, string> OpenapiCode = new()
        {
            //{ 100, "Continue 指示客户端可能继续其请求" },
            //{ 101, "SwitchingProtocols 指示正在更改协议版本或协议" },
            { 200, "OK 请求成功，且请求的信息包含在响应中。" },
            { 201, "Created 请求成功并且服务器创建了新的资源。" },
            { 202, "Accepted 服务器已接受请求，但尚未处理。" },
            { 203, "NonAuthoritativeInformation 返回的元信息来自缓存副本而不是原始服务器，因此可能不正确。" },
            { 204, "NoContent 服务器成功处理了请求，但没有返回任何内容。" },
            { 205, "ResetContent 客户端应重置（或重新加载）当前资源。" },
            { 206, "PartialContent 已部分下载了一个文件，可以续传损坏的下载。" },
            { 300, "MultipleChoices 请求的信息有多种表示形式。" },
            { 301, "MovedPermanently 请求的信息已永久移动到 Location 头中指定的 URI。" },
            { 302, "Redirect 服务器目前使用其它位置的网页响应请求，但请求者应继续使用原有位置来进行以后的请求。" },
            { 303, "SeeOther 将客户端自动重定向到 Location 头中指定的 URI。" },
            { 304, "NotModified 客户端的缓存副本是最新的。 未传输此资源的内容。" },
            { 305, "UseProxy 请求应使用位于 Location 头中指定的 URI 的代理服务器进行访问。" },
            { 307, "TemporaryRedirect 服务器目前使用其它位置的网页响应请求，但请求者应继续使用原有位置来进行以后的请求。" },
            { 400, "BadRequest 服务器未能识别请求。" },
            { 401, "Unauthorized 请求的资源要求身份验证。" },
            { 403, "Forbidden 服务器拒绝请求。" },
            { 404, "NotFound 服务器找不到请求的资源。" },
            { 405, "MethodNotAllowed 禁止使用当前指定的方法进行请求。" },
            { 406, "NotAcceptable 无法使用请求的内容特性响应请求。" },
            { 407, "ProxyAuthenticationRequired 表示请求的代理要求身份验证。" },
            { 408, "RequestTimeout 服务器等候请求时发生超时。" },
            { 409, "Conflict 服务器在完成请求时发生冲突。 服务器必须在响应中包含有关冲突的信息。" },
            { 410, "Gone 请求的资源已永久删除。" },
            { 411, "LengthRequired 缺少必需的 Content-length 头。" },
            { 429, "频率限制" },
            { 500, "InternalServerError 服务器遇到错误，无法完成请求。" },
            { 501, "NotImplemented 服务器不支持请求的函数方法。" },
            { 502, "BadGateway 服务器作为网关或代理，从上游服务器收到无效响应。" },
            { 503, "ServiceUnavailable 服务器暂时不可用，通常是由于过载或维护。" },
            { 504, "GatewayTimeout 服务器作为网关或代理，但是没有及时从上游服务器收到请求。" },
            { 505, "HttpVersionNotSupported 服务器不支持请求的HTTP 版本。" },
            { 4001, "无效的 opcode" },
            { 4002, "无效的 payload" },
            { 4006, "无效的 session_id，无法继续 resume，请重新发起 identify" },
            { 4007, "seq 错误" },
            { 4008, "发送 payload 过快，请重新连接，并遵守连接后返回的频控信息" },
            { 4009, "连接过期，请重连" },
            { 4010, "无效的 shard" },
            { 4011, "连接需要处理的 guild 过多，请进行合理的分片" },
            { 4012, "无效的 version" },
            { 4013, "无效的 intent" },
            { 4014, "intent 无权限" },
            { 4900, " 4900~4913 内部错误，请重连" },
            { 4914, "机器人已下架,只允许连接沙箱环境,请断开连接,检验当前连接环境" },
            { 4915, "机器人已封禁,不允许连接,请断开连接,申请解封后再连接" },
            { 10001, "UnknownAccount 账号异常" },
            { 10003, "UnknownChannel 子频道异常" },
            { 10004, "UnknownGuild 频道异常" },
            { 11281, "ErrorCheckAdminFailed 检查是否是管理员失败，系统错误，一般重试一次会好，最多只能重试一次" },
            { 11282, "ErrorCheckAdminNotPass 检查是否是管理员未通过，该接口需要管理员权限，但是用户在添加机器人的时候并未授予该权限，属于逻辑错误，可以提示用户进行授权" },
            { 11251, "ErrorWrongAppid 参数中的 appid 错误，开发者填的 token 错误，appid 无法识别" },
            { 11252, "ErrorCheckAppPrivilegeFailed 检查应用权限失败，系统错误，一般重试一次会好，最多只能重试一次" },
            { 11253, "ErrorCheckAppPrivilegeNotPass 检查应用权限不通过，该机器人应用未获得调用该接口的权限，需要向平台申请" },
            { 11254, "ErrorInterfaceForbidden 应用接口被封禁，该机器人虽然获得了该接口权限，但是被封禁了。" },
            { 11261, "ErrorWrongAppid 参数中缺少 appid，同 11251" },
            { 11262, "ErrorCheckRobot 当前接口不支持使用机器人 Bot Token 调用" },
            { 11263, "ErrorCheckGuildAuth 检查频道权限失败，系统错误，一般重试一次会好，最多只能重试一次" },
            { 11264, "ErrorGuildAuthNotPass 检查小站权限未通过，管理员添加机器人的时候未授予该接口权限，属于逻辑错误，可提示用户进行授权" },
            { 11265, "ErrorRobotHasBaned 机器人已经被封禁" },
            { 11241, "ErrorWrongToken 参数中缺少 token" },
            { 11242, "ErrorCheckTokenFailed 校验 token 失败，系统错误，一般重试一次会好，最多只能重试一次" },
            { 11243, "ErrorCheckTokenNotPass 校验 token 未通过，用户填充的 token 错误，需要开发者进行检查" },
            { 11273, "ErrorCheckUserAuth 检查用户权限失败，当前接口不支持使用 Bearer Token 调用" },
            { 11274, "ErrorUserAuthNotPass 检查用户权限未通过，用户 OAuth 授权时未给与该接口权限，可提示用户重新进行授权" },
            { 11275, "ErrorWrongAppid 无 appid ，同 11251" },
            { 12001, "ReplaceIDFailed 替换 id 失败" },
            { 12002, "RequestInvalid 请求体错误" },
            { 12003, "ResponseInvalid 回包错误" },
            { 20028, "ChannelHitWriteRateLimit 子频道消息触发限频" },
            { 50006, "CannotSendEmptyMessage 消息为空" },
            { 50035, "InvalidFormBody form-data 内容异常" },
            { 50037, "只能发送 markdown 消息" },
            { 50038, "非同频道同子频道" },
            { 50039, "获取消息失败" },
            { 50040, "消息模版类型错误" },
            { 50041, "markdown 有空值" },
            { 50042, "markdown 列表长达最大值" },
            { 50043, "guild_id 转换失败" },
            { 50045, "不能回复机器人自己产生的消息" },
            { 50046, "非 at 机器人消息" },
            { 50047, "非机器人产生的消息 或者 at 机器人消息" },
            { 301000, "参数错误" },
            { 301001, "查询频道信息错误" },
            { 301002, "查询子频道权限错误" },
            { 301003, "修改子频道权限错误" },
            { 301004, "私密子频道关联的人数到达上限" },
            { 301005, "调用 Rpc 服务失败" },
            { 301006, "非群成员没有查询权限" },
            { 301007, "参数超过数量限制" },
            { 302000, "参数错误" },
            { 302001, "查询频道信息错误" },
            { 302002, "查询日程列表失败" },
            { 302003, "查询日程失败" },
            { 302004, "修改日程失败" },
            { 302005, "删除日程失败" },
            { 302006, "创建日程失败" },
            { 302007, "获取创建者信息失败" },
            { 302008, "子频道 ID 不能为空" },
            { 302009, "频道系统错误，请联系客服" },
            { 302010, "暂无修改日程权限" },
            { 302011, "日程活动已被删除" },
            { 302012, "每天只能创建 10 个日程，明天再来吧！" },
            { 302013, "创建日程触发安全打击" },
            { 302014, "日程持续时间超过 7 天，请重新选择" },
            { 302015, "开始时间不能早于当前时间" },
            { 302016, "结束时间不能早于开始时间" },
            { 302017, "Schedule 对象为空" },
            { 302018, "参数类型转换失败" },
            { 302019, "调用下游失败，请联系客服" },
            { 302020, "日程内容违规、账号违规" },
            { 302021, "频道内当日新增活动达上限" },
            { 302022, "不能绑定非当前频道的子频道" },
            { 302023, "开始时跳转不可绑定日程子频道" },
            { 302024, "绑定的子频道不存在" },
            { 502000, "禁言相关错误" },
            { 502001, "频道 id 无效" },
            { 502002, "频道 id 为空" },
            { 502003, "用户 id 无效" },
            { 502004, "用户 id 为空" },
            { 502005, "timestamp 不合法" },
            { 502006, "timestamp 无效" },
            { 502007, "参数转换错误" },
            { 502008, "rpc 调用失败" },
            { 502009, "安全打击" },
            { 502010, "请求头错误" },
            { 304003, "URL_NOT_ALLOWED url 未报备" },
            { 304004, "ARK_NOT_ALLOWED 没有发 ark 消息权限" },
            { 304005, "EMBED_LIMIT embed 长度超限" },
            { 304006, "SERVER_CONFIG 后台配置错误" },
            { 304007, "GET_GUILD 查询频道异常" },
            { 304008, "GET_BOT 查询机器人异常" },
            { 304009, "GET_CHENNAL 查询子频道异常" },
            { 304010, "CHANGE_IMAGE_URL 图片转存错误" },
            { 304011, "NO_TEMPLATE 模板不存在" },
            { 304012, "GET_TEMPLATE 取模板错误" },
            { 304014, "TEMPLATE_PRIVILEGE 没有模板权限" },
            { 304016, "SEND_ERROR 发消息错误" },
            { 304017, "UPLOAD_IMAGE 图片上传错误" },
            { 304018, "SESSION_NOT_EXIST 机器人没连上 gateway" },
            { 304019, "AT_EVERYONE_TIMES @全体成员 次数超限" },
            { 304020, "FILE_SIZE 文件大小超限" },
            { 304021, "GET_FILE 下载文件错误" },
            { 304022, "PUSH_TIME 推送消息时间限制" },
            { 304023, "PUSH_MSG_ASYNC_OK 推送消息异步调用成功, 等待人工审核" },
            { 304024, "REPLY_MSG_ASYNC_OK 回复消息异步调用成功, 等待人工审核" },
            { 304025, "BEAT 消息被打击" },
            { 304026, "MSG_ID 回复的消息 id 错误" },
            { 304027, "MSG_EXPIRE 回复的消息过期" },
            { 304028, "MSG_PROTECT 非 At 当前用户的消息不允许回复" },
            { 304029, "CORPUS_ERROR 调语料服务错误" },
            { 304030, "CORPUS_NOT_MATCH 语料不匹配" },
            { 306001, "param invalid 撤回消息参数错误" },
            { 306002, "msgid error 消息 id 错误" },
            { 306003, "fail to get message 获取消息错误(可重试)" },
            { 306004, "no permission to delete message 没有撤回此消息的权限" },
            { 306005, "retract message error 消息撤回失败(可重试)" },
            { 306006, "fail to get channel 获取子频道失败(可重试)" },
            { 500000, "公告错误" },
            { 501001, "参数校验失败" },
            { 501002, "创建子频道公告失败(可重试)" },
            { 501003, "删除子频道公告失败(可重试)" },
            { 501004, "获取频道信息失败(可重试)" },
            { 501005, "MessageID 错误" },
            { 501006, "创建频道全局公告失败(可重试)" },
            { 501007, "删除频道全局公告失败(可重试)" },
            { 501008, "MessageID 不存在" },
            { 610000, "频道权限错误" },
            { 610001, "获取频道 ID 失败" },
            { 610002, "获取 HTTP 头失败" },
            { 610003, "获取机器人号码失败" },
            { 610004, "获取机器人角色失败" },
            { 610005, "获取机器人角色内部错误" },
            { 610006, "拉取机器人权限列表失败" },
            { 610007, "机器人不在频道内" },
            { 610008, "无效参数" },
            { 610009, "获取 API 接口详情失败" },
            { 610010, "API 接口已授权" },
            { 610011, "获取机器人信息失败" },
            { 610012, "限频失败" },
            { 610013, "已限频" },
            { 610014, "api 授权链接发送失败" },
            { 1000000, "发消息错误" },
            { 1100100, "安全打击：消息被限频" },
            { 1100101, "安全打击：内容涉及敏感，请返回修改" },
            { 1100102, "安全打击：抱歉，暂未获得新功能体验资格" },
            { 1100103, "安全打击" },
            { 1100104, "安全打击：该群已失效或当前群已不存在" },
            { 1100300, "系统内部错误" },
            { 1100301, "调用方不是群成员" },
            { 1100302, "获取指定频道名称失败" },
            { 1100303, "主页频道非管理员不允许发消息" },
            { 1100304, "@次数鉴权失败" },
            { 1100305, "TinyId 转换 Uin 失败" },
            { 1100306, "非私有频道成员" },
            { 1100307, "非白名单应用子频道" },
            { 1100308, "触发频道内限频" },
            { 1100499, "其他错误" },
        };
    }

    /// <summary>
    /// API请求状态
    /// </summary>
    public class ApiStatus
    {
        /// <summary>
        /// 代码
        /// </summary>
        [JsonPropertyName("code")]
        public int Code { get; set; }
        /// <summary>
        /// 原因
        /// </summary>
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    /// <summary>
    /// API请求出错的关键信息
    /// </summary>
    public class ApiErrorInfo
    {
        /// <summary>
        /// API请求出错的关键信息
        /// </summary>
        /// <param name="path">接口地址</param>
        /// <param name="method">请求方式</param>
        /// <param name="code">错误代码</param>
        /// <param name="detail">错误详情</param>
        /// <param name="freezeTime">接口被暂时停用的时间</param>
        public ApiErrorInfo(string path, string method, int code, string detail, FreezeTime freezeTime)
        {
            Path = path;
            Method = method;
            Code = code;
            Detail = detail;
            FreezeTime = freezeTime;
        }
        /// <summary>
        /// 接口地址
        /// </summary>
        public string Path { get; init; }
        /// <summary>
        /// 请求方式
        /// </summary>
        public string Method { get; init; }
        /// <summary>
        /// 错误代码
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string Detail { get; set; }
        /// <summary>
        /// 接口被暂时停用的时间
        /// </summary>
        public FreezeTime FreezeTime { get; init; }
    }
}
