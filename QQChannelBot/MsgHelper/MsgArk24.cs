using QQChannelBot.Models;

namespace QQChannelBot.MsgHelper
{
    /// <summary>
    /// 模板消息 id=24
    /// <para>文本、缩略图模板</para>
    /// </summary>
    public class MsgArk24 : MessageToCreate
    {
        /// <summary>
        /// 构造模板消息
        /// </summary>
        /// <param name="desc">描述</param>
        /// <param name="prompt">提示</param>
        /// <param name="title">标题</param>
        /// <param name="metaDesc">详情</param>
        /// <param name="image">图片URL</param>
        /// <param name="link">跳转链接</param>
        /// <param name="subTitle">子标题</param>
        /// <param name="replyMsgId">要回复的消息id</param>
        public MsgArk24(
            string? desc = null,
            string? prompt = null,
            string? title = null,
            string? metaDesc = null,
            string? image = null,
            string? link = null,
            string? subTitle = null,
            string? replyMsgId = null
            )
        {
            MsgId = replyMsgId;
            Desc = desc;
            Prompt = prompt;
            Title = title;
            MetaDesc = metaDesc;
            Img = image;
            Link = link;
            SubTitle = subTitle;
            Ark = new()
            {
                TemplateId = 24,
                Kv = new()
                {
                    ArkDesc,
                    ArkPrompt,
                    ArkTitle,
                    ArkMetaDesc,
                    ArkImage,
                    ArkLink,
                    ArkSubTitle
                }
            };
        }
        private readonly MessageArkKv ArkDesc = new() { Key = "#DESC#", Value = null };
        private readonly MessageArkKv ArkPrompt = new() { Key = "#PROMPT#", Value = null };
        private readonly MessageArkKv ArkTitle = new() { Key = "#TITLE#", Value = null };
        private readonly MessageArkKv ArkMetaDesc = new() { Key = "#METADESC#", Value = null };
        private readonly MessageArkKv ArkImage = new() { Key = "#IMG#", Value = null };
        private readonly MessageArkKv ArkLink = new() { Key = "#LINK#", Value = null };
        private readonly MessageArkKv ArkSubTitle = new() { Key = "#SUBTITLE#", Value = null };

        /// <summary>
        /// 设置要回复的目标消息
        /// </summary>
        /// <param name="msgId">目标消息的Id</param>
        /// <returns></returns>
        public MsgArk24 SetReplyMsgId(string? msgId) { MsgId = msgId; return this; }
        /// <summary>
        /// 描述
        /// </summary>
        public string? Desc { get => ArkDesc.Value; set => ArkDesc.Value = value; }
        /// <summary>
        /// 设置描述
        /// </summary>
        /// <param name="desc">描述内容</param>
        /// <returns></returns>
        public MsgArk24 SetDesc(string? desc) { Desc = desc; return this; }
        /// <summary>
        /// 提示
        /// </summary>
        public string? Prompt { get => ArkPrompt.Value; set => ArkPrompt.Value = value; }
        /// <summary>
        /// 设置提示
        /// </summary>
        /// <param name="prompt">提示内容</param>
        /// <returns></returns>
        public MsgArk24 SetPrompt(string? prompt) { Prompt = prompt; return this; }
        /// <summary>
        /// 标题
        /// </summary>
        public string? Title { get => ArkTitle.Value; set => ArkTitle.Value = value; }
        /// <summary>
        /// 设置标题
        /// </summary>
        /// <param name="title">标题内容</param>
        /// <returns></returns>
        public MsgArk24 SetTitle(string? title) { Title = title; return this; }
        /// <summary>
        /// 详情
        /// </summary>
        public string? MetaDesc { get => ArkMetaDesc.Value; set => ArkMetaDesc.Value = value; }
        /// <summary>
        /// 设置详情
        /// </summary>
        /// <param name="metaDesc">详情内容</param>
        /// <returns></returns>
        public MsgArk24 SetMetaDesc(string? metaDesc) { MetaDesc = metaDesc; return this; }
        /// <summary>
        /// 图片URL
        /// </summary>
        public string? Img { get => ArkImage.Value; set => ArkImage.Value = value; }
        /// <summary>
        /// 设置图片
        /// </summary>
        /// <param name="imgLink">图片URL</param>
        /// <returns></returns>
        public MsgArk24 SetImage(string? imgLink) { Img = imgLink; return this; }
        /// <summary>
        /// 跳转链接
        /// </summary>
        public string? Link { get => ArkLink.Value; set => ArkLink.Value = value; }
        /// <summary>
        /// 设置链接
        /// </summary>
        /// <param name="link">跳转链接</param>
        /// <returns></returns>
        public MsgArk24 SetLink(string? link) { Link = link; return this; }
        /// <summary>
        /// 子标题
        /// <para><em>子标题显示在模板消息底部</em></para>
        /// </summary>
        public string? SubTitle { get => ArkSubTitle.Value; set => ArkSubTitle.Value = value; }
        /// <summary>
        /// 设置子标题
        /// </summary>
        /// <param name="subTitle">子标题内容</param>
        /// <returns></returns>
        public MsgArk24 SetSubTitle(string? subTitle) { SubTitle = subTitle; return this; }
    }
}
