using AutoMapper;
using SparrowPlatform.Application.ViewModels;
using SparrowPlatform.Domain.Models;

namespace SparrowPlatform.Application.AutoMapper
{
    /// <summary>
    /// Transformation from entity model to view model.
    /// </summary>
    public class DomainToVoMappingProfile : Profile
    {
        public DomainToVoMappingProfile()
        {
            CreateMap<UserInfo, UserResponse>()
               ;
        }
    }
}
