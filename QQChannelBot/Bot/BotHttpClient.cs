using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
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
        public static async Task<HttpResponseMessage?> SendAsync(HttpRequestMessage request, Action<HttpResponseMessage, FreezeTime>? action = null)
        {
            string reqUrl = request.RequestUri!.ToString();
            if (FreezeUrl.TryGetValue(reqUrl, out FreezeTime? freezeTime) && freezeTime.EndTime > DateTime.Now)
            {
                Log.Warn($"[HttpSend] 目标接口处于冻结状态，暂时无法访问：{reqUrl}");
                return null;
            }
            HttpResponseMessage response = await HttpClient.SendAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                if (FreezeUrl.ContainsKey(reqUrl)) FreezeUrl.Remove(reqUrl);
                return response;
            }
            if (response.Content.Headers.ContentType?.MediaType == "application/json")
            {
                string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                ApiStatus? err = JsonSerializer.Deserialize<ApiStatus>(responseContent);
                if (err?.Code >= 400)
                {
                    if (FreezeUrl.TryGetValue(reqUrl, out freezeTime))
                    {
                        freezeTime.AddTime *= 2;
                        if (freezeTime.AddTime > FreezeMaxTime) freezeTime.AddTime = FreezeMaxTime;
                        freezeTime.EndTime = DateTime.Now + freezeTime.AddTime;
                    }
                    else
                    {
                        freezeTime = new FreezeTime() { AddTime = TimeSpan.FromSeconds(5) };
                        // 重点打击11264和11265错误，无接口访问权限；轻微处理其它错误
                        freezeTime.EndTime = DateTime.Now + ((err?.Code == 11264 || err?.Code == 11265) ? FreezeAddTime : freezeTime.AddTime);
                    }
                }
            }
            freezeTime ??= new FreezeTime() { EndTime = DateTime.Now, AddTime = TimeSpan.FromSeconds(5) };
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
            return await HttpClient.SendAsync(request).ConfigureAwait(false);
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
            return await HttpClient.SendAsync(request).ConfigureAwait(false);
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
            return await HttpClient.SendAsync(request).ConfigureAwait(false);
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
            return await HttpClient.SendAsync(request).ConfigureAwait(false);
        }
    }
    /// <summary>
    /// HttpClient请求拦截器
    /// </summary>
    public class HttpLoggingHandler : DelegatingHandler
    {
        const int printLength = 1024;
        /// <summary>
        /// HttpClient请求拦截器构造函数
        /// </summary>
        /// <param name="innerHandler"></param>
        public HttpLoggingHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }
        /// <summary>
        /// 发起异步Http请求
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string requestString = Regex.Replace(request.ToString(), @"(?<=Bot\s+)[^\n]+", (m) => Regex.Replace(m.Groups[0].Value, @"[^\.]", "*")); // 敏感信息脱敏
            string requestContent = request.Content != null ? await request.Content.ReadAsStringAsync() : "";
            MediaTypeHeaderValue? requestContentType = request.Content?.Headers.ContentType;
            if (requestContent.Length > printLength) requestContent = requestContent[..printLength];
            if ((requestContentType?.CharSet != null) || (requestContentType?.MediaType == "application/json")) { }
            else if (string.IsNullOrWhiteSpace(requestContent)) requestContent = "（没有内容）";
            else requestContent = "（内容无法解码）";
            requestContent = $"[HttpHandler][Request]{Environment.NewLine}{requestString}{Environment.NewLine}{requestContent}";

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                Log.Error(requestContent + "\n请求已取消！");
                return response; // 请求已取消
            }

            string responseString = response.ToString();
            string responseContent = response.Content != null ? await response.Content.ReadAsStringAsync() : "";
            HttpStatusCode responseStatusCode = response.StatusCode;
            MediaTypeHeaderValue? responseContentType = response.Content?.Headers.ContentType;

            if (responseContent.Length > printLength) responseContent = responseContent[..printLength];
            if ((responseContentType?.CharSet != null) || (responseContentType?.MediaType == "application/json")) { }
            else if (string.IsNullOrWhiteSpace(responseContent)) responseContent = "（没有内容）";
            else responseContent = "（内容无法解码）";
            responseContent = $"[HttpHandler][Response]{Environment.NewLine}{responseString}{Environment.NewLine}{responseContent}{Environment.NewLine}";
            if (responseStatusCode < HttpStatusCode.BadRequest) Log.Debug(requestContent + '\n' + responseContent);
            else Log.Error(requestContent + '\n' + responseContent);
            return response;
        }
    }
}
