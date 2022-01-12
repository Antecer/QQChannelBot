using System.Collections.Concurrent;

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
        public static LogLevel LogLevel { get; set; } = LogLevel.INFO;
        /// <summary>
        /// 时间格式化器
        /// <para>定义日志输出的时间戳格式 (默认值 HH:mm:ss.fff)</para>
        /// </summary>
        public static string TimeFormatter { get; set; } = "HH:mm:ss.f";

        /// <summary>
        /// 获取格式化的日期标签
        /// </summary>
        private static string TimeStamp { get => DateTime.Now.ToString(TimeFormatter); }
        /// <summary>
        /// 日志输出队列
        /// </summary>
        private static readonly ConcurrentQueue<string> LogQueue = new();

        private static bool IsWorking = false;
        /// <summary>
        /// 打印日志
        /// </summary>
        public static void Print(string message)
        {
            Task.Run(() =>
            {
                LogQueue.Enqueue(message);
                if (IsWorking) return;
                IsWorking = true;
                while (LogQueue.TryDequeue(out string? msg)) Console.WriteLine(msg);
                IsWorking = false;
            });
        }

        /// <summary>
        /// 打印调试
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message)
        {
            if (LogLevel == LogLevel.DEBUG) Print($"[{TimeStamp}][D]{message}");
        }

        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="message"></param>
        public static void Info(string message)
        {
            if (LogLevel <= LogLevel.INFO) Print($"[{TimeStamp}][I]{message}");
        }

        /// <summary>
        /// 打印警告
        /// </summary>
        /// <param name="message"></param>
        public static void Warn(string message)
        {
            if (LogLevel <= LogLevel.WARRNING) Print($"[{TimeStamp}][W]{message}");
        }

        /// <summary>
        /// 打印错误
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message)
        {
            if (LogLevel <= LogLevel.ERROR) Print($"[{TimeStamp}][E]{message}");
        }
    }

    /// <summary>
    /// 日志记录等级
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 调试
        /// </summary>
        DEBUG,
        /// <summary>
        /// 消息
        /// </summary>
        INFO,
        /// <summary>
        /// 警告
        /// </summary>
        WARRNING,
        /// <summary>
        /// 错误
        /// </summary>
        ERROR
    }
}
