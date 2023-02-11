using AutoMapper;
using SparrowPlatform.Application.ViewModels;
using SparrowPlatform.Domain.Models;

namespace SparrowPlatform.Application.AutoMapper
{
    /// <summary>
    /// Transformation from view model to entity model.
    /// </summary>
    public class VoToDomainMappingProfile : Profile
    {
        public VoToDomainMappingProfile()
        {
            CreateMap<UserUpdateVo, UserInfo>();
            CreateMap<UserVo, UserInfo>();
            CreateMap<AccountVo, AccountInfo>();
            CreateMap<ApplicationVo, Domain.Models.Application>();
            CreateMap<ApplicationInfosVo, Domain.Models.ApplicationInfos>();
            CreateMap<RoleVo, RoleInfo>();

            CreateMap<AccountVo, AccountExport>()
                .ForMember(a => a.EditAt, o => o.MapFrom(d => d.EditAt.ObjDate2String()));


            CreateMap<AccountVo, AccountExportZh>()
                .ForMember(a => a.EditAt, o => o.MapFrom(d => d.EditAt.ObjDate2String()));

            CreateMap<UserAccountVo, AccountInfo>()
                .ForMember(a => a.AccountName, o => o.MapFrom(d => d.name));
        }
    }
}
