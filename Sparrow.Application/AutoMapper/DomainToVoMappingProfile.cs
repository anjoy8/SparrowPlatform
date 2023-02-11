using AutoMapper;
using SparrowPlatform.Application.ViewModels;
using SparrowPlatform.Domain.Models;
using System.Linq;

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
                .ForMember(a => a.Role,
                o => o.MapFrom(x => x.RoleInfo.IsDeleted == false ? new UserRoleVo()
                {
                    id = x.RoleInfo.Id,
                    name = x.RoleInfo.Name
                } : null))
                .ForMember(a => a.Accounts,
                o => o.MapFrom(x => x.Accounts.Select(d =>
                    new UserAccountVo()
                    {
                        id = d.Id,
                        name = d.AccountName
                    })));
            CreateMap<AccountInfo, AccountVo>();
            CreateMap<RoleInfo, RoleVo>();
            CreateMap<Domain.Models.Application, ApplicationVo>();
            CreateMap<Domain.Models.ApplicationInfos, ApplicationInfosVo>();
        }
    }
}
