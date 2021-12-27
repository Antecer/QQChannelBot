using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QQChannelBot.MsgHelper
{
    /// <summary>
    /// 消息内嵌标签
    /// <para>仅作用于content中</para>
    /// </summary>
    public static class MsgTag
    {
        /// <summary>
        /// 创建 @用户 标签
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <returns></returns>
        public static string UserTag(string? userId = null)
        {
            return userId == null ? "" : $"<@!{userId}>";
        }

        /// <summary>
        /// 创建 #子频道 标签
        /// </summary>
        /// <param name="channelId">子频道id</param>
        /// <returns></returns>
        public static string ChannelTag(string? channelId = null)
        {
            return channelId == null ? "" : $"<#{channelId}>";
        }
    }
}
