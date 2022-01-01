﻿using System.Net;
using System.Text.Json;
using QQChannelBot.Bot.StatusCode;
using QQChannelBot.Tools;

namespace QQChannelBot.Bot
{
    /// <summary>
    /// 时间冻结类
    /// </summary>
    public class FreezeTime
    {
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; } = DateTime.MinValue;
        /// <summary>
        /// 附加时间
        /// </summary>
        public TimeSpan AddTime { get; set; } = TimeSpan.Zero;
    }
    /// <summary>
    /// 经过封装的HttpClient
    /// <para>内置了请求日志功能</para>
    /// </summary>
    public static class BotHttpClient
    {
        /// <summary>
        /// URL访问失败的默认冻结时间
        /// </summary>
        public static TimeSpan FreezeAddTime { get; set; } = TimeSpan.FromSeconds(30);
        /// <summary>
        /// URL访问失败的最高冻结时间
        /// </summary>
        public static TimeSpan FreezeMaxTime { get; set; } = TimeSpan.FromHours(1);
        /// <summary>
        /// 临时冻结无权限访问的URL
        /// <para>
        /// value.Item1 - 解封时间(DateTime)
        /// value.Item2 - 再次封禁增加的时间(TimeSpan)
        /// </para>
        /// </summary>
        private static readonly Dictionary<string, FreezeTime> FreezeUrl = new();
        /// <summary>
        /// Http客户端
        /// <para>这里设置禁止重定向：AllowAutoRedirect = false</para>
        /// <para>这里设置超时时间为15s</para>
        /// </summary>
        public static HttpClient HttpClient { get; } = new(new HttpLoggingHandler(new HttpClientHandler() { AllowAutoRedirect = false })) { Timeout = TimeSpan.FromSeconds(15) };

        /// <summary>
        /// 发起HTTP异步请求
        /// </summary>
        /// <param name="request">请求消息</param>
        /// <param name="action">请求失败的回调函数</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage?> SendAsync(HttpRequestMessage request, Action<HttpResponseMessage, FreezeTime?>? action = null)
        {
            string reqUrl = request.RequestUri!.ToString();
            if (FreezeUrl.TryGetValue(reqUrl, out var freezeTime) && freezeTime.EndTime > DateTime.Now) return null;
            HttpResponseMessage response = await HttpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                if (FreezeUrl.ContainsKey(reqUrl)) FreezeUrl.Remove(reqUrl);
                return response;
            }
            if (response.Content.Headers.ContentType?.MediaType == "application/json")
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                ApiError? err = JsonSerializer.Deserialize<ApiError>(responseContent);
                // 打击11264和11265错误，无接口访问权限
                if (err?.Code == 11264 || err?.Code == 11265)
                {
                    if (FreezeUrl.TryGetValue(reqUrl, out freezeTime))
                    {
                        TimeSpan freezeNext = freezeTime.AddTime * 2;
                        freezeTime.EndTime = DateTime.Now + (freezeNext > FreezeMaxTime ? FreezeMaxTime : freezeNext);
                        FreezeUrl[reqUrl] = freezeTime;
                    }
                    else
                    {
                        freezeTime = new FreezeTime() { EndTime = DateTime.Now + FreezeAddTime, AddTime = FreezeAddTime };
                        FreezeUrl[reqUrl] = freezeTime;
                    }
                }
            }
            action?.Invoke(response, freezeTime);
            return null;
        }

        /// <summary>
        /// HTTP异步GET
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage?> GetAsync(string url)
        {
            HttpRequestMessage request = new() { RequestUri = new Uri(url), Content = null, Method = HttpMethod.Get };
            return await HttpClient.SendAsync(request);
        }

        /// <summary>
        /// HTTP异步Post
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="content">请求内容</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage?> PostAsync(string url, HttpContent content)
        {
            HttpRequestMessage request = new() { RequestUri = new Uri(url), Content = content, Method = HttpMethod.Post };
            return await HttpClient.SendAsync(request);
        }

        /// <summary>
        /// HTTP异步Put
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="content">请求内容</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage?> PutAsync(string url, HttpContent content)
        {
            HttpRequestMessage request = new() { RequestUri = new Uri(url), Content = content, Method = HttpMethod.Put };
            return await HttpClient.SendAsync(request);
        }

        /// <summary>
        /// HTTP异步Delete
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="content">请求内容</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage?> DeleteAsync(string url, HttpContent content)
        {
            HttpRequestMessage request = new() { RequestUri = new Uri(url), Content = content, Method = HttpMethod.Delete };
            return await HttpClient.SendAsync(request);
        }
    }
    /// <summary>
    /// HttpClient请求拦截器
    /// </summary>
    public class HttpLoggingHandler : DelegatingHandler
    {
        const int printLength = 2048;
        public HttpLoggingHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (Log.LogLevel == LogLevel.Debug)
            {
                string requestContent = request.Content != null ? await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false) : "{}";
                if (requestContent.Length > printLength) requestContent = Unicoder.Decode(requestContent[..printLength]);
                requestContent = $"[HttpHandler] Request:{Environment.NewLine}{request}{Environment.NewLine}{requestContent}";
                Log.Debug(requestContent);
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            if ((Log.LogLevel == LogLevel.Debug) && (response.StatusCode >= HttpStatusCode.BadRequest))
            {
                string responseContent = response.Content != null ? await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false) : "{}";
                if ((response.Content?.Headers.ContentType?.CharSet != null) || (response.Content?.Headers.ContentType?.MediaType == "application/json"))
                {
                    if (responseContent.Length > printLength) responseContent = Unicoder.Decode(responseContent[..printLength]);
                }
                responseContent = $"[HttpHandler] Response:{Environment.NewLine}{response}{Environment.NewLine}{responseContent}{Environment.NewLine}";
                if (response.StatusCode < HttpStatusCode.BadRequest) Log.Debug(responseContent);
                else Log.Error(responseContent);
            }
            return response;
        }
    }
}
