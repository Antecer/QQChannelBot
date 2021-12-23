using QQChannelBot.Models;

namespace QQChannelBot.MsgHelper
{
    /// <summary>
    /// 模板消息 id=23
    /// <para>链接+文本列表模板</para>
    /// </summary>
    public class MsgArk23 : MessageToCreate
    {
        public MsgArk23(string replyMsgId = "")
        {
            MsgId = replyMsgId;
            Ark = new()
            {
                TemplateId = 23,
                Kv = new()
                {
                    ArkDesc,
                    ArkPrompt,
                    new() { Key = "#LIST#", Obj = MsgLines }
                }
            };
        }
        private readonly MessageArkKv ArkDesc = new() { Key = "#DESC#", Value = null };
        private readonly MessageArkKv ArkPrompt = new() { Key = "#PROMPT#", Value = null };

        /// <summary>
        /// 设置要回复的目标消息
        /// </summary>
        /// <param name="msgId">目标消息的Id</param>
        /// <returns></returns>
        public MsgArk23 SetReplyMsgId(string msgId) { MsgId = msgId; return this; }
        /// <summary>
        /// 描述
        /// </summary>
        public string? Desc { get => ArkDesc.Value; set => ArkDesc.Value = value; }
        /// <summary>
        /// 设置描述
        /// </summary>
        /// <param name="desc">描述内容</param>
        /// <returns></returns>
        public MsgArk23 SetDesc(string desc) { Desc = desc; return this; }
        /// <summary>
        /// 提示消息
        /// </summary>
        public string? Prompt { get => ArkPrompt.Value; set => ArkPrompt.Value = value; }
        /// <summary>
        /// 设置提示
        /// </summary>
        /// <param name="prompt">提示内容</param>
        /// <returns></returns>
        public MsgArk23 SetPrompt(string prompt) { Prompt = prompt; return this; }
        /// <summary>
        /// 内容列表
        /// </summary>
        public List<MessageArkObj> MsgLines { get; set; } = new();
        /// <summary>
        /// 添加一行内容
        /// <para>
        /// content - 本行要显示的文字<br/>
        /// link - 本行文字绑定的超链接(URL需要审核通过才能用)
        /// </para>
        /// </summary>
        /// <param name="content">内容描述</param>
        /// <param name="link">内容链接 [可选]</param>
        /// <returns></returns>
        public MsgArk23 AddLine(string content, string? link = null)
        {
            List<MessageArkObjKv> ojbk = new List<MessageArkObjKv>()
            {
                new(){Key = "desc", Value = content}
            };
            if (link != null) ojbk.Add(new() { Key = "link", Value = link });

            MsgLines.Add(new() { ObjKv = ojbk });
            return this;
        }
    }
}
