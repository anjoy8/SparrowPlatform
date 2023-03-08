using AutoMapper;
using SparrowPlatform.Application.ViewModels;
using SparrowPlatform.Domain.Models;

namespace SparrowPlatform.Application.AutoMapper
{
    /// <summary>
    /// Transformation all view models to entity models.
    /// </summary>
    public class VoToDomainMappingProfile : Profile
    {
        public VoToDomainMappingProfile()
        {
            CreateMap<UserUpdateVo, UserInfo>();
            CreateMap<UserVo, UserInfo>();
          
        }
    }
}
