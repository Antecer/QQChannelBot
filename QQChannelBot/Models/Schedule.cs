using System.Text.Json.Serialization;

namespace QQChannelBot.Models
{
    /// <summary>
    /// 日程对象
    /// </summary>
    public class Schedule
    {
        /// <summary>
        /// 构造日程
        /// </summary>
        public Schedule() { }
        /// <summary>
        /// 新建日程
        /// <para>
        /// 注1：开始时间必须大于当前时间<br/>
        /// 注2：结束时间必须大于开始时间<br/>
        /// 注3：调用API每日创建日程数量有限
        /// </para>
        /// </summary>
        /// <param name="name">日程名称</param>
        /// <param name="desc">日程描述</param>
        /// <param name="startTime">开始时间（默认五分钟后）</param>
        /// <param name="endTime">结束时间（默认持续一小时）</param>
        /// <param name="jumpChannel">日程开始时跳转的频道</param>
        /// <param name="remindType">日程时间即将到达时的提醒方式</param>
        public Schedule(
            string name = "新建日程",
            string desc = "新的日程",
            DateTime? startTime = null,
            DateTime? endTime = null,
            Channel? jumpChannel = null,
            ScheduleRemindType remindType = ScheduleRemindType.Never)
        {
            Name = name;
            Description = desc;
            StartTimestamp = new DateTimeOffset(startTime ?? DateTime.Now.AddMinutes(5)).ToUnixTimeMilliseconds().ToString();
            EndTimestamp = new DateTimeOffset(endTime ?? DateTime.Now.AddMinutes(65)).ToUnixTimeMilliseconds().ToString();
            JumpChannelId = jumpChannel?.Id;
            RemindType = remindType;
        }
        /// <summary>
        /// 日程 id
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }
        /// <summary>
        /// 日程名称
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        /// <summary>
        /// 日程描述
        /// </summary>
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        /// <summary>
        /// 日程开始时间戳(ms)
        /// <para>必须大于当前时间</para>
        /// </summary>
        [JsonPropertyName("start_timestamp")]
        public string? StartTimestamp { get; set; }
        /// <summary>
        /// 日程结束时间戳(ms)
        /// <para>必须大于开始时间</para>
        /// </summary>
        [JsonPropertyName("end_timestamp")]
        public string? EndTimestamp { get; set; }
        /// <summary>
        /// 创建者
        /// </summary>
        [JsonPropertyName("creator")]
        public Member? Creator { get; set; }
        /// <summary>
        /// 日程开始时跳转到的子频道 id
        /// </summary>
        [JsonPropertyName("jump_channel_id")]
        public string? JumpChannelId { get; set; }
        /// <summary>
        /// 日程提醒类型
        /// <para>请勿直接读写此属性，而是应该通过RemindType属性读写</para>
        /// </summary>
        [JsonPropertyName("remind_type")]
        public string Remind { get; set; } = "0";
        /// <summary>
        /// 日程提醒类型
        /// </summary>
        [JsonIgnore]
        public ScheduleRemindType? RemindType
        {
            get => (ScheduleRemindType)int.Parse(Remind);
            set => Remind = value?.ToString("D") ?? "0";
        }
    }

    /// <summary>
    /// 日程提醒方式
    /// </summary>
    public enum ScheduleRemindType
    {
        /// <summary>
        /// 不提醒
        /// </summary>
        Never,
        /// <summary>
        /// 开始时提醒
        /// </summary>
        OnStart,
        /// <summary>
        /// 开始前5分钟提醒
        /// </summary>
        Early5Min,
        /// <summary>
        /// 开始前15分钟提醒
        /// </summary>
        Early15Min,
        /// <summary>
        /// 开始前30分钟提醒
        /// </summary>
        Early30Min,
        /// <summary>
        /// 开始前60分钟提醒
        /// </summary>
        Early60Min
    }
}
