using SparrowPlatform.Application.AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace SparrowPlatform.API.Extensions
{
    /// <summary>
    /// Dependency injection configuration of AutoMapper.
    /// </summary>
    public static class AutoMapperSetup
    {
        public static void AddAutoMapperSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            services.AddAutoMapper(typeof(AutoMapperConfig));
            AutoMapperConfig.RegisterMappings();
        }
    }
}
