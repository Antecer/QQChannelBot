using System.Text.Json;
using System.Text.RegularExpressions;

namespace QQChannelBot.Bot
{
    /// <summary>
    /// 经过封装的HttpClient
    /// <para>内置了请求日志功能</para>
    /// </summary>
    public static class BotHttpClient
    {
        /// <summary>
        /// Http客户端
        /// <para>这里设置禁止重定向：AllowAutoRedirect = false</para>
        /// </summary>
        public static HttpClient HttpClient { get; } = new(new HttpLoggingHandler(new HttpClientHandler() { AllowAutoRedirect = false }));

        /// <summary>
        /// 发起HTTP异步请求
        /// </summary>
        /// <param name="request">请求消息</param>
        /// <param name="action">请求失败的回调函数</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage?> SendAsync(HttpRequestMessage request, Action<HttpResponseMessage>? action = null)
        {
            HttpResponseMessage response = await HttpClient.SendAsync(request);
            if (response.IsSuccessStatusCode) return response;
            action?.Invoke(response);
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
        public HttpLoggingHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string requestContent = request.Content != null ? await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false) : "{}";
            if (requestContent.Length > 10240) requestContent = requestContent[..10240];
            Log.Debug(Regex.Unescape($"[HttpHandler] Request:{Environment.NewLine}{request}{Environment.NewLine}{requestContent}"));

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            string responseContent = response.Content != null ? await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false) : "";
            if (string.IsNullOrWhiteSpace(responseContent)) responseContent = "{}";
            if (responseContent.Length > 10240) responseContent = responseContent[..10240];
            Log.Debug(Regex.Unescape($"[HttpHandler] Response:{Environment.NewLine}{response}{Environment.NewLine}{responseContent}{Environment.NewLine}"));
            return response;
        }
    }
}
