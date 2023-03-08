using SparrowPlatform.Application.Services;
using SparrowPlatform.Infrastruct.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NETCore.MailKit.Core;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SparrowPlatform.IntegrateApi.Job.Extensions
{
    /// <summary>
    /// job service.
    /// </summary>
    public class JobTimedUserService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;

        public JobTimedUserService(IServiceScopeFactory scopeFactory, IEmailService emailService, IWebHostEnvironment env)
        {
            _scopeFactory = scopeFactory;
            _emailService = emailService;
            _env = env;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("JobTimedUserService 开始启动");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(1 * 60 * 60));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            try
            {
                var exTime = ExecutionPlan.current.UserQz.CycleTime.ObjToInt();
                var nowTime = DateTime.Now.Hour + 8;
                if (exTime > 0 && nowTime == exTime)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var _govTrafficService = scope.ServiceProvider.GetRequiredService<IUserService>();
                        var data = _govTrafficService.GetAll();
                        if (data?.Any() == true)
                        {
                            // TODO 执行业务逻辑
                        }
                        Console.WriteLine($"JobTimedUserService 执行完成");
                    }
                }
                else
                {
                    Console.WriteLine($"JobTimedUserService execution time has not arrived: {exTime}:00 -- {nowTime}:00");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JobTimedUserService 70 Code Error:{ex.Message}");
            }

            Console.WriteLine($"JobTimedUserService： {DateTime.Now}");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("JobTimedUserService 停止");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }

}
