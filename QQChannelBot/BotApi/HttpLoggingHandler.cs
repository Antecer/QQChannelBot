using System.Text.RegularExpressions;

namespace QQChannelBot.BotApi
{
    /// <summary>
    /// HttpClient请求拦截器
    /// </summary>
    public class HttpLoggingHandler : DelegatingHandler
    {
        public HttpLoggingHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Log.Debug(Regex.Unescape($"[HttpHandler] Request:{Environment.NewLine}{request}{Environment.NewLine}{(request.Content != null ? await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false) : "{}")}{Environment.NewLine}"));

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            Log.Debug(Regex.Unescape($"[HttpHandler] Response:{Environment.NewLine}{response}{Environment.NewLine}{(response.Content != null ? await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false) : "{}")}{Environment.NewLine}"));

            return response;
        }
    }
}
