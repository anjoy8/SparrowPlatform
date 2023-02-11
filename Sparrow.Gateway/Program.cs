using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace SparrowPlatform.Gateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
                    var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
                    var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
                    var vaultUri = Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_URI");
                    config
                     .AddJsonFile("appsettings.json", true)
                     .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true)
                     .AddEnvironmentVariables();

                    config.AddJsonFile("ocelot.json", optional: true, reloadOnChange: true);
                    config.AddAzureKeyVault(vaultUri, clientId, clientSecret);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
