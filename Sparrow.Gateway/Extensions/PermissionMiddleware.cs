using SparrowPlatform.Application.ViewModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SparrowPlatform.Gateway.Extensions
{
    public class PermissionMiddleware
    {

        private readonly RequestDelegate next;
        private readonly IHttpClientFactory _httpclientFatory;
        private Stopwatch _stopwatch;

        public PermissionMiddleware(RequestDelegate next, IHttpClientFactory httpclientFatory)
        {
            this.next = next;
            this._httpclientFatory = httpclientFatory;
            _stopwatch = new Stopwatch();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var apiPath = context.Request.Path.Value.ToLower();
            Console.WriteLine("PermissionMiddleware " + apiPath);

            // 白名单
            if (AppsetIntegrateGateway.OcelotSet.AuthWhiteList.Contains(apiPath))
            {
                Console.WriteLine("PermissionMiddleware " + apiPath + " auth white list");
                await next.Invoke(context);
                return;
            }

            if (apiPath.Contains("/api/") && !apiPath.Contains("login"))
            {
                _stopwatch.Restart();
                var rlt = IsPermissioned(context);
                _stopwatch.Stop();
                var permissTime = _stopwatch.ElapsedMilliseconds + "ms";
                Console.WriteLine($"{DateTime.Now}, latency analysis 1, apiPath: {apiPath}, apiVerify, latency={permissTime};");


                if (rlt != null && rlt.code == 200 && rlt.data != null && rlt.data.userId > 0)
                {
                    try
                    {
                        context.Request.Headers.Add("x-deals", rlt.data.deals);
                        context.Request.Headers.Add("x-userId", rlt.data.userId.ToString());
                        Console.WriteLine(JsonConvert.SerializeObject(rlt.data));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"PermissionMiddleware error:{e.Message}");
                    }
                    await next.Invoke(context);
                    return;
                }

                Console.WriteLine($"request 400: {apiPath} not permissioned;");
                // Return unauthorized
                context.Response.StatusCode = 400;
                await HandleExceptionAsync(context, "Not allowed");
            }
            else
            {
                await next.Invoke(context);
            }
        }

        public ApiPermissionVerify IsPermissioned(HttpContext context)
        {
            try
            {
                var client = _httpclientFatory.CreateClient("AuthServiceClient");
                string application = context?.Request?.Headers["X-Application"] ?? "";
                var token = GetTokenStringFromHeader(context);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                client.DefaultRequestHeaders.Add("X-Application", application);

                var apiPath = context.Request.Path.Value.TrimStart('/').TrimEnd('/');
                var httpMethod = context.Request.Method.ToLower();

                var requestJson = JsonConvert.SerializeObject(new ApiPermissionVerifyRequest(apiPath, httpMethod));
                using (HttpContent httpContent = new StringContent(requestJson))
                {
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var rlt = client.PostAsync($"/api/Sparrow-auth/application/apiVerify", httpContent).Result;
                    var rltJson = rlt.Content.ReadAsStringAsync().Result;
                    var rltData = JsonConvert.DeserializeObject<ApiPermissionVerify>(rltJson);
                    return rltData;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"IsPermissioned 400: {e.Message} \r\n {e.StackTrace}");
                return new ApiPermissionVerify();
            }
        }

        private string GetTokenStringFromHeader(HttpContext context)
        {
            var token = string.Empty;
            string authorization = context?.Request?.Headers["Authorization"];

            if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith($"Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authorization.Substring("Bearer ".Length).Trim();
            }

            return token;
        }
        private async Task HandleExceptionAsync(HttpContext httpContext, string msg)
        {

            var error = new ApiResultVo<string>()
            {
                status = httpContext?.Response?.StatusCode ?? 400,
                msg = msg,
                response = null,
                success = false,
            };
            var result = JsonConvert.SerializeObject(error);
            Console.WriteLine($"HandleExceptionAsync 400: {result}");
            httpContext.Response.ContentType = "application/json;charset=utf-8";
            await httpContext.Response.WriteAsync(result).ConfigureAwait(false);
        }
    }
    public static class PermissionExtensions
    {
        public static IApplicationBuilder UsePermission(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PermissionMiddleware>();
        }
    }

    public class ApiPermissionVerifyRequest
    {
        public ApiPermissionVerifyRequest(string apiPath, string httpMethod)
        {
            this.apiPath = apiPath;
            this.httpMethod = httpMethod;
        }

        public string apiPath { get; set; }
        public string httpMethod { get; set; }
    }
    public class ApiPermissionVerify
    {
        public int code { get; set; } = 0;
        public string message { get; set; }
        public ApiVerifyResponse data { get; set; }
    }
    public class ApiVerifyResponse
    {
        public string deals { get; set; }
        public int userId { get; set; }
    }

}
