namespace QQChannelBot.Bot
{
    /// <summary>
    /// 鉴权信息
    /// </summary>
    public struct Identity
    {
        /// <summary>
        /// 机器人Id
        /// </summary>
        public string BotAppId { get; set; }
        /// <summary>
        /// 机器人Token
        /// </summary>
        public string BotToken { get; set; }
        /// <summary>
        /// 机器人密钥
        /// </summary>
        public string BotSecret { get; set; }
    }
}
