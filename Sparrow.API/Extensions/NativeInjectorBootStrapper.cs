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
    /// all dependent injection configuration.
    /// </summary>
    public class NativeInjectorBootStrapper
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddHostedService<JobTimedUserService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAuthorizationHandler, ClaimsRequirementHandler>();

            services.AddScoped<IUserService, UserService>();

            services.AddScoped<IUserRepository, UserRepository>();

            services.AddScoped<SparrowPlatformDbContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

    }
}
