namespace QQChannelBot.Models
{
    /// <summary>
    /// 日程
    /// </summary>
    public class Schedule
    {
        /// <summary>
        /// 日程 id
        /// </summary>
        public string? Id { get; set; }
        /// <summary>
        /// 日程名称
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// 日程描述
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// 日程开始时间戳(ms)
        /// </summary>
        public string? StartTimestamp { get; set; }
        /// <summary>
        /// 日程结束时间戳(ms)
        /// </summary>
        public string? EndTimestamp { get; set; }
        /// <summary>
        /// 创建者
        /// </summary>
        public Member? Creator { get; set; }
        /// <summary>
        /// 日程开始时跳转到的子频道 id
        /// </summary>
        public string? JumpChannelId { get; set; }
        /// <summary>
        /// 日程提醒类型，取值参考RemindType
        /// <para>
        /// 0 - 不提醒 <br/>
        /// 1 - 开始时提醒 <br/>
        /// 2 - 开始前5分钟提醒 <br/>
        /// 3 - 开始前15分钟提醒 <br/>
        /// 4 - 开始前30分钟提醒 <br/>
        /// 5 - 开始前60分钟提醒 <br/>
        /// </para>
        /// </summary>
        public string? RemindType { get; set; }
    }
}
