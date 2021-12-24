using System.Text.Json;
using System.Text.RegularExpressions;
using QQChannelBot.BotApi.StatusCode;

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
            string requestContent = request.Content != null ? await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false) : "{}";
            Log.Debug(Regex.Unescape($"[HttpHandler] Request:{Environment.NewLine}{request}{Environment.NewLine}{requestContent}"));

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            string responseContent = response.Content != null ? await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false) : "{}";
            Log.Debug(Regex.Unescape($"[HttpHandler] Response:{Environment.NewLine}{response}{Environment.NewLine}{responseContent}{Environment.NewLine}"));

            if (response.IsSuccessStatusCode) return response;
            // 全局捕获Http通信错误
            try
            {
                ApiError? err = JsonSerializer.Deserialize<ApiError>(responseContent);
                if (err?.Code != null)
                {
                    string errStr = $"代码：{err?.Code} 内容：{(StatusCodes.OpenapiCode.TryGetValue(err?.Code ?? -999, out string? errMsg) ? errMsg : null) ?? "此错误类型未收录!"}";
                    response.StatusCode = System.Net.HttpStatusCode.OK;
                    response.Content = null;
                    throw new Exception(errStr);
                }
            }
            catch (Exception e)
            {
                Log.Error($"[接口访问失败] {e.Message}{Environment.NewLine}");
            }
            return response;
        }
    }

}
