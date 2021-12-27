namespace QQChannelBot.Bot
{
    /// <summary>
    /// 集中处理日志信息
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// 日志记录级别
        /// </summary>
        public static LogLevel LogLevel { get; set; } = LogLevel.Info;
        /// <summary>
        /// 时间格式化器
        /// <para>定义日志输出的时间戳格式 (默认值 HH:mm:ss.fff)</para>
        /// </summary>
        public static string TimeFormatter { get; set; } = "HH:mm:ss.fff";

        /// <summary>
        /// 获取格式化的日期标签
        /// </summary>
        private static string TimeStamp { get => DateTime.Now.ToString(TimeFormatter); }

        /// <summary>
        /// 打印调试
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(object message)
        {
            if (LogLevel == LogLevel.Debug) Console.WriteLine($"[{TimeStamp}][D] {message}");
        }

        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="message"></param>
        public static void Info(object message)
        {
            if (LogLevel <= LogLevel.Info) Console.WriteLine($"[{TimeStamp}][I] {message}");
        }

        /// <summary>
        /// 打印警告
        /// </summary>
        /// <param name="message"></param>
        public static void Warn(object message)
        {
            if (LogLevel <= LogLevel.Warning) Console.WriteLine($"[{TimeStamp}][W] {message}");
        }

        /// <summary>
        /// 打印错误
        /// </summary>
        /// <param name="message"></param>
        public static void Error(object message)
        {
            if (LogLevel <= LogLevel.Error) Console.WriteLine($"[{TimeStamp}][E] {message}");
        }
    }

    /// <summary>
    /// 日志记录等级
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
}
