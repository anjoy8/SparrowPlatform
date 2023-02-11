using SparrowPlatform.API.Extensions;
using SparrowPlatform.API.Utils;
using SparrowPlatform.Infrastruct.Data;
using SparrowPlatform.Infrastruct.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Rong.EasyExcel.Npoi;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.IdentityModel.Tokens.Jwt;
using IGeekFan.AspNetCore.Knife4jUI;

namespace SparrowPlatform.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            Permissions.USE_GATEWAY = bool.Parse(Configuration["Startup:Permission:UseGateway"]);

            CallBackInfo callBackInfo = new();
            Configuration.Bind("CallBackInfo", callBackInfo);

            ExecutionPlan.current = Configuration.GetSection("ExecutionPlan").Get<ExecutionPlan>();
            AppSetting.current = Configuration.GetSection("AppSetting").Get<AppSetting>();

            services.AddCustomServices(Configuration);

            //if (!Permissions.USE_GATEWAY)
            {
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                       .AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAdB2C"));
            }

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Authorizor.OnlyCanReadDemo, policy => policy.Requirements.Add(new ClaimRequirement("extension_UserRole", "admin")));
            });

            services.AddControllers();

            var mssqlUserName = Configuration.GetValue<string>("MSSQL-USERNAME");
            var mssqlUserPwd = Configuration.GetValue<string>("MSSQL-PASSWORD");
            var conn = Configuration.GetValue<string>("MysqlDbConnectionStrings");
            conn = string.Format(conn, mssqlUserName, mssqlUserPwd);
            services.AddDbContext<SparrowPlatformDbContext>(o => o
                //.UseLazyLoadingProxies()
                .UseSqlServer(conn));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SparrowPlatform.API",
                    Version = "v1",
                    Description = $"Version Time: {DateTime.Now.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")}"
                }); ;
                c.OperationFilter<AddResponseHeadersFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                c.OperationFilter<SecurityRequirementsOperationFilter>();
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization (data will be transmitted in the request header). Enter Bearer {token} directly in the box below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
            });

            services.AddNpoiExcel();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUser, HttpContextUser>();
            //Add MailKit
            var notificationMetadata = Configuration.GetSection("MailKitConfig").Get<MailKitOptions>();
            notificationMetadata.Password = Configuration.GetValue<string>("MAIL-KIT-PWD");
            services.AddMailKit(optionBuilder =>
            {
                optionBuilder.UseMailKit(notificationMetadata);
            });

            services.AddHttpClient();
            services.AddHttpClient("AuthServiceClient", client =>
            {
                client.DefaultRequestHeaders.Add("client-name", "auth");
                client.BaseAddress = new Uri("http://auth-service:6390");
            });

            services.AddAutoMapperSetup();
            NativeInjectorBootStrapper.RegisterServices(services);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            System.Console.WriteLine("Service started");
            System.Console.WriteLine(env.EnvironmentName);

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SparrowPlatform.API v1"));
            app.UseKnife4UI(c =>
            {
                c.SwaggerEndpoint($"../swagger/v1/swagger.json", $"SparrowPlatform.API v1");
                c.RoutePrefix = "spadoc";
            });

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.Map("/common/debug", HandleDebug);
            app.Map("/common/auth", HandleAuth);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static void HandleDebug(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                Console.WriteLine($"{context.Request.Headers["X-Forwarded-For"]}-{context.Connection.RemoteIpAddress}");
                var sign = context.Request.Query["sign"];
                if (sign == "open")
                {
                    AAD_SET.Debug = true;
                }
                if (sign == "close")
                {
                    AAD_SET.Debug = false;
                }

                await context.Response.WriteAsync($"AAD_SET.Debug = {AAD_SET.Debug}");
            });
        }
        private static void HandleAuth(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                var sign = context.Request.Query["sign"];
                if (sign == "open")
                {
                    Permissions.USE_GATEWAY = true;
                }
                if (sign == "close")
                {
                    Permissions.USE_GATEWAY = false;
                }

                await context.Response.WriteAsync($"Permissions.USE_GATEWAY = {Permissions.USE_GATEWAY}");
            });
        }
    }
    public static class CustomExtensions
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration Configuration)
        {
            AAD_SET.Debug = bool.Parse(Configuration["Startup:AAD:Debug"]);

            AzureAdB2CSetup.Instance = Configuration["AzureAdB2C:Instance"];
            AzureAdB2CSetup.ClientId = Configuration["AzureAdB2C:ClientId"];
            AzureAdB2CSetup.Domain = Configuration["AzureAdB2C:Domain"];
            AzureAdB2CSetup.SignUpSignInPolicyId = Configuration["AzureAdB2C:SignUpSignInPolicyId"];

            AzureADAppSetup.application = Configuration["AzureADAppSetup:application"];
            AzureADAppSetup.domain = Configuration["AzureADAppSetup:domain"];
            AzureADAppSetup.loginDomain = Configuration["AzureADAppSetup:loginDomain"];

            AzureADAppSetup.connectionString = Configuration["AzureADAppSetup:connectionString"];
            var DATABRICKSACCOUNTKEY = Configuration.GetValue<string>("DATABRICKS-ACCOUNT-KEY");
            AzureADAppSetup.connectionString = string.Format(AzureADAppSetup.connectionString, DATABRICKSACCOUNTKEY);

            AzureADAppSetup.blobFileDownloadConnectionString = Configuration["AzureADAppSetup:blobFileDownloadConnectionString"];
            var BLOBACCOUNTKEYBILL = Configuration.GetValue<string>("BLOB-ACCOUNT-KEY-BILL");
            AzureADAppSetup.blobFileDownloadConnectionString = string.Format(AzureADAppSetup.blobFileDownloadConnectionString, BLOBACCOUNTKEYBILL);



            AzureADAppSetup.b2cExtensionsApplicationClientID = Configuration["AzureADAppSetup:b2cExtensionsApplicationClientID"];
            AzureADAppSetup.blobAccountName = Configuration["AzureADAppSetup:blobAccountName"];

            AzureADAppTokenSetup.grantType = Configuration["AzureADAppTokenSetup:grantType"];
            AzureADAppTokenSetup.clientId = Configuration["AzureADAppTokenSetup:clientId"];

            AzureADAppTokenSetup.clientSecret = Configuration["AzureADAppTokenSetup:clientSecret"];
            AzureADAppTokenSetup.clientSecret = Configuration.GetValue<string>("AAD-CLIENT-SECRET");

            AzureADAppTokenSetup.scope = Configuration["AzureADAppTokenSetup:scope"];

            return services;
        }
    }
}
