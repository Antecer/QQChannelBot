using System.Reflection;

namespace QQChannelBot.Bot
{
    /// <summary>
    /// SDK信息
    /// </summary>
    public static class InfoSDK
    {
        private static AssemblyName sdk = typeof(BotClient).Assembly.GetName();
        /// <summary>
        /// SDK名称
        /// </summary>
        public static string? Name => sdk.Name;
        /// <summary>
        /// SDK版本
        /// </summary>
        public static Version? Version => sdk.Version;
        /// <summary>
        /// SDK开源地址HTTPS
        /// </summary>
        public static string GitHTTPS => "https://github\uFEFF.com/Antecer/QQChannelBot";
        /// <summary>
        /// SDK开源地址SSH
        /// </summary>
        public static string GitSSH => @"git@github.com:Antecer/QQChannelBot.git";
        /// <summary>
        /// 版权信息
        /// </summary>
        public static string Copyright => "Copyright © 2021 Antecer. All rights reserved.";
    }
}
