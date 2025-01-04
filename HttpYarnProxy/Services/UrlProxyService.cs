using HttpYarnProxy.Entities;
using System.Diagnostics;
using System.Net.Http;
using System.Text;

namespace HttpYarnProxy.Services
{
    /// <summary>
    /// Url代理服务
    /// </summary>
    public class UrlProxyService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UrlProxyService> _logger;
        private static readonly string[] NonContainHeaders = { "Connection", "Content-Type", "Content-Length", "Host" };

        public UrlProxyService(HttpClient httpClient, ILogger<UrlProxyService> logger)// 实际为ihttpclientfactory构建的多个httpclient重复使用的httpmessagehandler
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task HttpClientRequestAsync(UrlProxyMap entity, HttpContext httpContext)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            // 启用请求体缓冲
            httpContext.Request.EnableBuffering();
            _logger.LogInformation("启用请求体缓冲");

            // 创建代理请求
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = GetMethod(string.IsNullOrEmpty(entity.RequestType) ? "get" : entity.RequestType),
                RequestUri = new Uri(entity.Url)
            };
            _logger.LogInformation("开始复制请求内容和头");
            stopwatch.Restart();
            // 复制请求内容和头
            await CopyRequestContentAndHeadersAsync(httpContext, httpRequestMessage);
            stopwatch.Stop();
            _logger.LogInformation($"结束复制请求内容和头:{stopwatch.Elapsed.TotalMilliseconds} ms");
            // 如果有请求体，设置内容
            if (!string.IsNullOrEmpty(entity.ParamsJson))
            {
                httpRequestMessage.Content = new StringContent(entity.ParamsJson, Encoding.UTF8, "application/json");
            }
            stopwatch.Restart();
            _logger.LogInformation("发送请求并处理响应");
            // 发送请求并处理响应
            await SendAsync(httpContext, httpRequestMessage);
            stopwatch.Stop();
            _logger.LogInformation($"结束请求并处理响应:{stopwatch.Elapsed.TotalMilliseconds} ms");
        }

        private async Task SendAsync(HttpContext context, HttpRequestMessage requestMessage)
        {
            var gid = Guid.NewGuid();
            try
            {
                _logger.LogInformation($"发送请求:{gid}");
                // 
                //var handler = GetHttpClientHandler(_httpClient);
                //Console.WriteLine($"HttpClientHandler HashCode: {handler.GetHashCode()}");

                using (var responseMessage = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                {
                    // 设置响应状态码
                    context.Response.StatusCode = (int)responseMessage.StatusCode;
                    // 复制响应头
                    foreach (var header in responseMessage.Headers)
                    {
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                    }

                    foreach (var header in responseMessage.Content.Headers)
                    {
                        context.Response.Headers[header.Key] = header.Value.ToArray();
                    }

                    // 移除不必要的头
                    context.Response.Headers.Remove("transfer-encoding");
                    // 复制响应体
                    await responseMessage.Content.CopyToAsync(context.Response.Body);
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError($"请求超时:{gid}>{_httpClient.Timeout}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        private static HttpMessageHandler GetHttpClientHandler(HttpClient httpClient)
        {
            var handler = httpClient;
            return null;
        }

        private static HttpMethod GetMethod(string method)
        {
            if (HttpMethods.IsDelete(method)) return HttpMethod.Delete;
            if (HttpMethods.IsGet(method)) return HttpMethod.Get;
            if (HttpMethods.IsHead(method)) return HttpMethod.Head;
            if (HttpMethods.IsOptions(method)) return HttpMethod.Options;
            if (HttpMethods.IsPost(method)) return HttpMethod.Post;
            if (HttpMethods.IsPut(method)) return HttpMethod.Put;
            if (HttpMethods.IsTrace(method)) return HttpMethod.Trace;
            return new HttpMethod(method);
        }

        private async Task CopyRequestContentAndHeadersAsync(HttpContext context, HttpRequestMessage requestMessage)
        {
            var requestMethod = context.Request.Method;

            // 复制请求体（如果不是 GET、HEAD、DELETE 或 TRACE 请求）
            if (!HttpMethods.IsGet(requestMethod) &&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod) &&
                !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(context.Request.Body);
                requestMessage.Content = streamContent;
            }

            // 复制请求头
            foreach (var header in context.Request.Headers)
            {
                if (!NonContainHeaders.Contains(header.Key))
                {
                    if (header.Key != "User-Agent")
                    {
                        if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && requestMessage.Content != null)
                        {
                            requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                        }
                    }
                    else
                    {
                        string userAgent = header.Value.Count > 0 ? header.Value[0] + " " + context.TraceIdentifier : string.Empty;

                        if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, userAgent) && requestMessage.Content != null)
                        {
                            requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, userAgent);
                        }
                    }
                }
            }
        }
    }
}
