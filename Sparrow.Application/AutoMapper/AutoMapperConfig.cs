using AutoMapper;

namespace SparrowPlatform.Application.AutoMapper
{
    /// <summary>
    /// Object mapping configuration.
    /// </summary>
    public class AutoMapperConfig
    {
        public static MapperConfiguration RegisterMappings()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new DomainToVoMappingProfile());
                cfg.AddProfile(new VoToDomainMappingProfile());
            });
        }
    }
}
