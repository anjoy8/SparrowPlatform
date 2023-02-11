using AutoMapper;
using SparrowPlatform.Application.ViewModels;
using SparrowPlatform.Domain.Interfaces;
using SparrowPlatform.Domain.Models;
using SparrowPlatform.Infrastruct.Utils;
using NETCore.MailKit.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace SparrowPlatform.Application.Services
{
    /// <summary>
    /// User service
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleInfoRepository _roleInfoRepository;
        private readonly IAccountInfoRepository _accountInfoRepository;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IApplicationRepository _applicationInfoRepository;
        private readonly IRoleApplicationRepository _roleApplicationRepository;
        private readonly IEmailService _emailService;
        private readonly IHttpClientFactory _httpclientFatory;
        private readonly IHttpContextAccessor _accessor;
        private readonly IUser _user;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly string extensionAttribute = "userRole";

        public UserService(IUserRepository userRepository,
            IRoleInfoRepository roleInfoRepository,
            IAccountInfoRepository accountInfoRepository,
            IUserAccountRepository userTenantRepository,
            IApplicationRepository applicationInfoRepository,
            IRoleApplicationRepository roleApplicationRepository,
            IEmailService emailService,
            IHttpClientFactory httpclientFatory,
            IHttpContextAccessor accessor,
            IUser user,
            IMapper mapper, IUnitOfWork uow)
        {
            _userRepository = userRepository;
            _roleInfoRepository = roleInfoRepository;
            this._accountInfoRepository = accountInfoRepository;
            this._userAccountRepository = userTenantRepository;
            this._applicationInfoRepository = applicationInfoRepository;
            this._roleApplicationRepository = roleApplicationRepository;
            this._emailService = emailService;
            this._httpclientFatory = httpclientFatory;
            this._accessor = accessor;
            this._user = user;
            _mapper = mapper;
            _uow = uow;
        }

        public ApiResultVo<UserResponse> AddOne(UserVo userVo)
        {
            if (userVo == null)
            {
                return ApiResultVo<UserResponse>.error("参数不能为空！");
            }

            if (string.IsNullOrEmpty(userVo.Login))
            {
                return ApiResultVo<UserResponse>.error("Login不能为空！");
            }

            if (string.IsNullOrEmpty(userVo.Password))
            {
                return ApiResultVo<UserResponse>.error("Password不能为空！");
            }
            if (userVo.Password != userVo.ConfirmPassword)
            {
                return ApiResultVo<UserResponse>.error("确认密码与密码不一致！");
            }

            if (!(userVo.Role.id > 0))
            {
                return ApiResultVo<UserResponse>.error("角色不能为空！");
            }

            var userInfo = _mapper.Map<UserInfo>(userVo);

            var userInfoModel = _userRepository.GetUserInfoByLogin(userVo.Login);
            if (userInfoModel != null)
            {
                return ApiResultVo<UserResponse>.error("This login is already existed.");
            }

            var selectRoleModel = _roleInfoRepository.GetById(userVo.Role.id);
            if (selectRoleModel == null)
            {
                return ApiResultVo<UserResponse>.error("选择的角色不存在！");
            }

            var identities = new List<Identity>() { };
            identities.Add(new Identity()
            {
                signInType = "userName",
                issuer = AzureAdB2CSetup.Domain,
                issuerAssignedId = userVo.Login,
            });

            var azureUserDto = AddExtensionAttribute(selectRoleModel.Name);
            azureUserDto.displayName = userVo.DisplayName;
            azureUserDto.passwordPolicies = "DisablePasswordExpiration";
            azureUserDto.passwordProfile = new Passwordprofile()
            {
                password = userVo.Password,
                forceChangePasswordNextSignIn = true,
            };
            azureUserDto.identities = identities;

            var aadInsert = AzureADApp.AddUserByToken(azureUserDto);
            if (aadInsert.suc)
            {
                if (!string.IsNullOrEmpty(userVo.Email))
                {
                    try
                    {
                        // TODO 执行业务逻辑

                    }
                    catch (Exception)
                    {
                        Console.WriteLine(userVo.Email + " email send failed");
                    }
                }

                userInfo.AADId = aadInsert.id;

                userInfo.Accounts = null;

                _userRepository.Add(userInfo);

                if (_uow.Commit())
                {
                    var uid = userInfo.Id;
                    if (userVo?.Accounts?.Any() == true)
                    {
                        var tenantIds = userVo.Accounts?.Select(d => d.id) ?? new List<string>();
                        if (uid.IsNotEmptyOrNull())
                        {
                            ConfigRelationUserTenant(uid, tenantIds.ToList());
                        }
                    }

                    return ApiResultVo<UserResponse>.ok(null, "添加成功！");
                }
            }
            else
            {
                return ApiResultVo<UserResponse>.error($"添加用户到AAD失败！({aadInsert.msg})");
            }

            return ApiResultVo<UserResponse>.error("添加失败！");
        }


        public ApiResultVo<UserResponse> Update(UserUpdateVo userVo)
        {
            if (userVo.Id.IsNotEmptyOrNull())
            {
                var userInfoModel = _userRepository.GetAll().Where(d => d.Id == userVo.Id && d.IsDeleted == false).FirstOrDefault();
                if (userInfoModel == null)
                {
                    return ApiResultVo<UserResponse>.error("该用户不存在！");
                }

                var selectRoleModel = _roleInfoRepository.GetById(userVo.Role.id);
                if (selectRoleModel == null)
                {
                    return ApiResultVo<UserResponse>.error("选择的角色不存在！");
                }

                if (!string.IsNullOrEmpty(userVo.Password))
                {
                    userInfoModel.Password = userVo.Password;
                }
                userInfoModel.Email = userVo.Email;
                userInfoModel.Remark = userVo.Remark;
                userInfoModel.DisplayName = userVo.DisplayName;
                userInfoModel.RoleId = userVo.Role.id;
                userInfoModel.DataScope = userVo.DataScope;
                userInfoModel.Validity = userVo.Validity;
                userInfoModel.ApplicationScopeAll = userVo.ApplicationScopeAll;
                userInfoModel.EditAt = userVo.EditAt;
                userInfoModel.EditBy = userVo.EditBy;
                if (userInfoModel.CreatedBy.ObjToString() == "")
                {
                    userInfoModel.CreatedBy = userInfoModel.EditBy;
                }

                var identities = new List<Identity>() { };
                identities.Add(new Identity()
                {
                    signInType = "userName",
                    issuer = AzureAdB2CSetup.Domain,
                    // login禁止修改
                    issuerAssignedId = userInfoModel.Login,
                });

                var azureUpdateUserDto = AddExtensionAttribute(selectRoleModel.Name);
                azureUpdateUserDto.displayName = userVo.DisplayName;

                if (!string.IsNullOrWhiteSpace(userVo.Password) && userVo.Password.Length > 7)
                {
                    azureUpdateUserDto.passwordPolicies = "DisablePasswordExpiration";
                    azureUpdateUserDto.passwordProfile = new Passwordprofile()
                    {
                        password = userVo.Password,
                        forceChangePasswordNextSignIn = true,
                    };
                }
                azureUpdateUserDto.identities = identities;


                var aadUpdate = AzureADApp.UpdateUserByToken(JsonConvert.SerializeObject(azureUpdateUserDto), userInfoModel.AADId);
                if (aadUpdate.suc)
                {
                    _userRepository.Update(userInfoModel);

                    if (_uow.Commit())
                    {
                        var uid = userInfoModel.Id;
                        var tenantIds = userVo.Accounts?.Select(d => d.id) ?? new List<string>();
                        if (uid.IsNotEmptyOrNull())
                        {
                            ConfigRelationUserTenant(uid, tenantIds.ToList());
                        }

                        return ApiResultVo<UserResponse>.ok(null, "更新成功！");
                    }
                }
                else
                {
                    return ApiResultVo<UserResponse>.error($"更新AAD失败！({aadUpdate.msg})");
                }
            }

            return ApiResultVo<UserResponse>.error("更新失败！");
        }
        public ApiResultVo<UserResponse> Delete(int Id)
        {
            if (Id.IsNotEmptyOrNull())
            {
                var userInfoModel = _userRepository.GetAll().Where(d => d.Id == Id && d.IsDeleted == false).FirstOrDefault();
                if (userInfoModel == null)
                {
                    return ApiResultVo<UserResponse>.error("该用户不存在！");
                }

                userInfoModel.IsDeleted = true;

                var isUpdated = AzureADApp.DeleteUserByToken(userInfoModel.AADId);

                if (isUpdated)
                {
                    _userRepository.Update(userInfoModel);

                    if (_uow.Commit())
                    {
                        return ApiResultVo<UserResponse>.ok(null, "删除成功！");
                    }
                }
                else
                {
                    return ApiResultVo<UserResponse>.error("删除AAD失败！");
                }
            }

            return ApiResultVo<UserResponse>.error("删除失败！");
        }

        public List<UserResponse> GetAll(Expression<Func<UserInfo, bool>> whereExpression = null)
        {
            if (whereExpression == null)
            {
                whereExpression = a => a.IsDeleted == false;
            }
            var data = _userRepository.GetAll().Where(whereExpression).ToList();

            var roles = _roleInfoRepository.GetAll().Where(d => d.IsDeleted == false).ToList();
            foreach (var item in data)
            {
                item.RoleInfo = roles.FirstOrDefault(d => d.Id == item.RoleId);
            }

            return _mapper.Map<List<UserResponse>>(data);
        }

        public List<UserResponse> GetPageList(RequestPages requestPages, Expression<Func<UserInfo, bool>> whereExpression = null)
        {
            if (whereExpression == null)
            {
                whereExpression = a => a.IsDeleted == false;
            }

            var data = _userRepository.GetPageList(whereExpression, requestPages).ToList();
            var roles = _roleInfoRepository.GetAll().Where(d => d.IsDeleted == false).ToList();
            var userAccounts = _userAccountRepository.GetAll().ToList();
            var tenants = _accountInfoRepository.GetAll().Where(d => d.IsDeleted == false).ToList();

            foreach (var item in data)
            {
                var tenantIds = userAccounts.Where(d => d.UserInfoId == item.Id.ToString()).Select(c => c.AccountInfoId).ToList();
                item.RoleInfo = roles.FirstOrDefault(d => d.Id == item.RoleId);
                item.Accounts = tenants.FindAll(d => tenantIds.Contains(d.Id));
            }

            return _mapper.Map<List<UserResponse>>(data);
        }

        public ApiResultVo<UserResponse> GetOne(int id)
        {
            var data = _userRepository.GetAll()
                .Where(d => d.Id == id && d.IsDeleted == false).FirstOrDefault();
            if (data != null)
            {
                data.RoleInfo = _roleInfoRepository.GetById(data.RoleId);

                // 1/3 全量
                if (data.DataScope)
                {
                    data.Accounts = _accountInfoRepository.GetAll().Where(d => d.IsDeleted == false).ToList();
                }
                else
                {
                    var allAccountList = _accountInfoRepository.GetAll();
                    // 2/3 关系表
                    var accountsAll = new List<AccountInfo>();
                    var tenantIds = _userAccountRepository.GetAll().Where(d => d.UserInfoId == data.Id.ToString()).Select(c => c.AccountInfoId).ToList();
                    var accounts1 = allAccountList.Where(d => tenantIds.Contains(d.Id) && d.IsDeleted == false).ToList();
                    if (accounts1 != null) accountsAll.AddRange(accounts1);

                    // 3/3 某个应用内全量
                    if (data.ApplicationScopeAll.IsNotEmptyOrNull())
                    {
                        var appOfUses = data.ApplicationScopeAll.Split(",");
                        List<string> appids = _applicationInfoRepository.GetAll().Where(d => appOfUses.Contains(d.Name)).Select(d => d.Id).ToList();

                        var accounts2 = allAccountList.Where(d => appids.Contains(d.ApplicationId) && d.IsDeleted == false).ToList();
                        if (accounts2 != null) accountsAll.AddRange(accounts2);
                    }

                    data.Accounts = accountsAll;
                }

                return ApiResultVo<UserResponse>.ok(_mapper.Map<UserResponse>(data));
            }
            return ApiResultVo<UserResponse>.error("该用户不存在！");
        }


        private int DateDiff(DateTime dateStart, DateTime dateEnd)
        {
            DateTime start = Convert.ToDateTime(dateStart.ToShortDateString());
            DateTime end = Convert.ToDateTime(dateEnd.ToShortDateString());
            TimeSpan sp = end.Subtract(start);
            return sp.Days;
        }



    }
    public class EmailAddRequest
    {
        public string application { get; set; }
        public string emailAddress { get; set; }
        public string emailContent { get; set; }
        public string function { get; set; }
        public int id { get; set; }
        public string sendTime { get; set; } = DateTime.Now.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");
    }
    public class AuditLog
    {
        public int code { get; set; }
        public string message { get; set; }
    }
}
