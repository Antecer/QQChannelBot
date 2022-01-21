using System.Collections.Concurrent;
using QQChannelBot.Tools;

namespace QQChannelBot.Bot
{
    /// <summary>
    /// 集中处理日志信息
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// 日志数据结构体
        /// </summary>
        /// <param name="Level"></param>
        /// <param name="Message"></param>
        /// <param name="TimeStamp"></param>
        public record struct LogEntry(LogLevel Level, string Message, string TimeStamp);
        /// <summary>
        /// 自定义日志输出
        /// </summary>
        public static event Action<LogEntry>? LogTo;
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
        private static string TimeStamp { get => $"[{DateTime.Now.ToString(TimeFormatter)}]"; }
        /// <summary>
        /// 日志输出队列
        /// </summary>
        private static readonly ConcurrentQueue<LogEntry> LogQueue = new();

        private static bool IsWorking = false;
        /// <summary>
        /// 打印日志
        /// </summary>
        private static void Print(LogEntry logItem)
        {
            Task.Run(() =>
            {
                LogQueue.Enqueue(logItem);
                if (IsWorking) return;
                IsWorking = true;
                while (LogQueue.TryDequeue(out LogEntry entry))
                {
                    if (LogLevel <= entry.Level)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(entry.TimeStamp);
                        Console.ForegroundColor = entry.Level switch
                        {
                            LogLevel.DEBUG => ConsoleColor.Gray,
                            LogLevel.INFO => ConsoleColor.DarkGreen,
                            LogLevel.WARRNING => ConsoleColor.DarkYellow,
                            LogLevel.ERROR => ConsoleColor.DarkRed,
                            _ => ConsoleColor.Magenta,
                        };
                        Console.WriteLine(Unicoder.Decode(entry.Message));
                        LogTo?.Invoke(entry);
                    }
                }
                IsWorking = false;
            });
        }

        /// <summary>
        /// 打印调试
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message) => Print(new(LogLevel.DEBUG, message, TimeStamp));

        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="message"></param>
        public static void Info(string message) => Print(new(LogLevel.INFO, message, TimeStamp));

        /// <summary>
        /// 打印警告
        /// </summary>
        /// <param name="message"></param>
        public static void Warn(string message) => Print(new(LogLevel.WARRNING, message, TimeStamp));

        /// <summary>
        /// 打印错误
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message) => Print(new(LogLevel.ERROR, message, TimeStamp));
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
