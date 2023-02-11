using SparrowPlatform.Account.Services;
using SparrowPlatform.Application.Services;
using SparrowPlatform.Domain.Interfaces;
using SparrowPlatform.Infrastruct.Data;
using SparrowPlatform.Infrastruct.Repositories;
using SparrowPlatform.Infrastruct.UoW;
using SparrowPlatform.IntegrateApi.Job.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace SparrowPlatform.API.Extensions
{
    /// <summary>
    /// All dependent injection configuration.
    /// </summary>
    public class NativeInjectorBootStrapper
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddHostedService<JobTimedUserService>();
            services.AddHostedService<JobTimedUserAdminService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAuthorizationHandler, ClaimsRequirementHandler>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAccountInfoService, AccountInfoService>();
            services.AddScoped<IApplicationService, ApplicationService>();
            services.AddScoped<IApplicationInfosService, ApplicationInfosService>();
            services.AddScoped<IRoleInfoService, RoleInfoService>();


            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserAccountRepository, UserAccountRepository>();
            services.AddScoped<IRoleInfoRepository, RoleInfoRepository>();
            services.AddScoped<IAccountInfoRepository, AccountInfoRepository>();
            services.AddScoped<IApplicationRepository, ApplicationRepository>();
            services.AddScoped<IApplicationInfosRepository, ApplicationInfosRepository>();
            services.AddScoped<IRoleApplicationRepository, RoleApplicationRepository>();

            services.AddScoped<SparrowPlatformDbContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

        }

    }
}
