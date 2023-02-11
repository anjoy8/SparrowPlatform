using SparrowPlatform.Application.ViewModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace SparrowPlatform.Gateway.Extensions
{
    public class CustomExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        public CustomExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex.Message, ex);
                await HandleExceptionAsync(httpContext, ex.Message);
            }
            finally
            {
                var statusCode = httpContext.Response.StatusCode;
                var msg = "";
                switch (statusCode)
                {
                    case 401:
                        msg = "未授权";
                        break;
                    case 403:
                        msg = "拒绝访问";
                        break;
                    case 404:
                        msg = "未找到服务";
                        break;
                    case 405:
                        msg = "405 Method Not Allowed";
                        break;
                    case 502:
                        msg = "请求错误";
                        break;
                }
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    await HandleExceptionAsync(httpContext, msg);
                }
            }
        }
        ///
        private async Task HandleExceptionAsync(HttpContext httpContext, string msg)
        {

            var error = new ApiResultVo<string>()
            {
                status = httpContext.Response.StatusCode,
                msg = msg,
                response = null,
                success = false,
            };
            var result = JsonConvert.SerializeObject(error);
            httpContext.Response.ContentType = "application/json;charset=utf-8";
            await httpContext.Response.WriteAsync(result).ConfigureAwait(false);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class CustomExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomExceptionMiddleware>();
        }
    }
}
