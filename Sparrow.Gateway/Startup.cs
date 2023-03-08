using SparrowPlatform.Gateway.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Kubernetes;
using System;
using System.Collections.Generic;

namespace SparrowPlatform.Gateway
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
            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddControllers();

            services.AddAuthentication(o =>
            {
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAdB2C"), "user-tenant-svc");

            services.AddOcelot()
                    .AddKubernetes();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SparrowPlatform.Gateway", Version = "v1" });
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddHttpClient();
            services.AddHttpClient("AuthServiceClient", client =>
            {
                client.BaseAddress = new Uri("http://auth-service:8080");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCustomExceptionMiddleware();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSession();

            DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();
            defaultFilesOptions.DefaultFileNames.Clear();
            defaultFilesOptions.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(defaultFilesOptions);
            app.UseStaticFiles();

            app.UseSwaggerAuthorized();

            app.UsePermission();

            app.UseSwagger();
            var apis = new List<string> { "UsersApi"};
            app.UseSwaggerUI(options =>
            {
                apis.ForEach(m =>
                {
                    options.SwaggerEndpoint($"/swagger/apiswg/{m}/swagger.json", m);
                });
            });


            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


            app.UseOcelot().Wait();
        }
    }
}
