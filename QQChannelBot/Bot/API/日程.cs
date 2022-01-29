using System.Net.Http.Json;
using QQChannelBot.Models;

namespace QQChannelBot.Bot
{
    public partial class BotClient
    {
        /// <summary>
        /// 获取频道日程列表
        /// <para>
        /// 获取某个日程子频道里中当天的日程列表; <br/>
        /// 若带了参数 since，则返回结束时间在 since 之后的日程列表; <br/>
        /// 若未带参数 since，则默认返回当天的日程列表。
        /// </para>
        /// </summary>
        /// <param name="channel_id">日程子频道Id</param>
        /// <param name="since">筛选日程开始时间（默认为当日全天）</param>
        /// <param name="sender"></param>
        /// <returns>List&lt;Schedule&gt;?</returns>
        public async Task<List<Schedule>?> GetSchedulesAsync(string channel_id, DateTime? since = null, Sender? sender = null)
        {
            string param = since == null ? "" : $"?since={new DateTimeOffset(since.Value).ToUnixTimeMilliseconds()}";
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/schedules{param}", null, null, sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<List<Schedule>?>();
        }
        /// <summary>
        /// 获取单个日程信息
        /// </summary>
        /// <param name="channel_id">日程子频道Id</param>
        /// <param name="schedule_id">日程Id</param>
        /// <param name="sender"></param>
        /// <returns>目标 Schedule 对象</returns>
        public async Task<Schedule?> GetScheduleAsync(string channel_id, string schedule_id, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/schedules/{schedule_id}", null, null, sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<Schedule?>();
        }
        /// <summary>
        /// 创建日程
        /// <para>
        /// 要求操作人具有"管理频道"的权限，如果是机器人，则需要将机器人设置为管理员。<br/>
        /// 创建成功后，返回创建成功的日程对象。<br/>
        /// 日程开始时间必须大于当前时间。
        /// </para>
        /// </summary>
        /// <param name="channel_id">日程子频道Id</param>
        /// <param name="schedule">新的日程对象，不需要带Id</param>
        /// <param name="sender"></param>
        /// <returns>新创建的 Schedule 对象</returns>
        public async Task<Schedule?> CreateScheduleAsync(string channel_id, Schedule schedule, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/schedules", HttpMethod.Post, JsonContent.Create(new { schedule }), sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<Schedule?>();
        }
        /// <summary>
        /// 修改日程
        /// <para>
        /// 要求操作人具有"管理频道"的权限，如果是机器人，则需要将机器人设置为管理员。<br/>
        /// 修改成功后，返回修改后的日程对象。
        /// </para>
        /// </summary>
        /// <param name="channel_id">日程子频道Id</param>
        /// <param name="schedule">修改后的日程对象</param>
        /// <param name="sender"></param>
        /// <returns>修改后的 Schedule 对象</returns>
        public async Task<Schedule?> EditScheduleAsync(string channel_id, Schedule schedule, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/schedules/{schedule.Id}", HttpMethod.Patch, JsonContent.Create(new { schedule }), sender);
            return respone == null ? null : await respone.Content.ReadFromJsonAsync<Schedule?>();
        }
        /// <summary>
        /// 删除日程
        /// <para>
        /// 要求操作人具有"管理频道"的权限，如果是机器人，则需要将机器人设置为管理员。
        /// </para>
        /// </summary>
        /// <param name="channel_id">日程子频道Id</param>
        /// <param name="schedule">日程对象
        /// <para>这里是为了获取日程Id，为了防错设计为传递日程对象</para></param>
        /// <param name="sender"></param>
        /// <returns>HTTP 状态码 204</returns>
        public async Task<bool> DeleteScheduleAsync(string channel_id, Schedule schedule, Sender? sender = null)
        {
            HttpResponseMessage? respone = await HttpSendAsync($"{ApiOrigin}/channels/{channel_id}/schedules/{schedule.Id}", HttpMethod.Delete, null, sender);
            return respone?.IsSuccessStatusCode ?? false;
        }
    }
}
