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
        private readonly IEmailService _emailService;
        private readonly IHttpClientFactory _httpclientFatory;
        private readonly IHttpContextAccessor _accessor;
        private readonly IUser _user;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly string extensionAttribute = "userRole";

        public UserService(IUserRepository userRepository,
            IEmailService emailService,
            IHttpClientFactory httpclientFatory,
            IHttpContextAccessor accessor,
            IUser user,
            IMapper mapper, IUnitOfWork uow)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _httpclientFatory = httpclientFatory;
            _accessor = accessor;
            _user = user;
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

            var userInfo = _mapper.Map<UserInfo>(userVo);

            var userInfoModel = _userRepository.GetUserInfoByLogin(userVo.Login);
            if (userInfoModel != null)
            {
                return ApiResultVo<UserResponse>.error("This login is already existed.");
            }

            var identities = new List<Identity>() { };
            identities.Add(new Identity()
            {
                signInType = "userName",
                issuer = AzureAdB2CSetup.Domain,
                issuerAssignedId = userVo.Login,
            });

            var azureUserDto = AddExtensionAttribute();
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

                _userRepository.Add(userInfo);

                if (_uow.Commit())
                {
                    var uid = userInfo.Id;

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

                if (!string.IsNullOrEmpty(userVo.Password))
                {
                    userInfoModel.Password = userVo.Password;
                }
                userInfoModel.Email = userVo.Email;
                userInfoModel.Remark = userVo.Remark;
                userInfoModel.DisplayName = userVo.DisplayName;
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

                var azureUpdateUserDto = AddExtensionAttribute();
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

        private dynamic AddExtensionAttribute(string role = "admin")
        {
            IDictionary<string, object> result = new ExpandoObject();

            var extensionAttr = $"extension_{AzureADAppSetup.b2cExtensionsApplicationClientID}_{extensionAttribute}";
            result.Add(extensionAttr, role);

            return result as ExpandoObject;
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

            return _mapper.Map<List<UserResponse>>(data);
        }

        public List<UserResponse> GetPageList(RequestPages requestPages, Expression<Func<UserInfo, bool>> whereExpression = null)
        {
            if (whereExpression == null)
            {
                whereExpression = a => a.IsDeleted == false;
            }

            var data = _userRepository.GetPageList(whereExpression, requestPages).ToList();
            return _mapper.Map<List<UserResponse>>(data);
        }

        public ApiResultVo<UserResponse> GetOne(int id)
        {
            var data = _userRepository.GetAll()
                .Where(d => d.Id == id && d.IsDeleted == false).FirstOrDefault();
            if (data != null)
            {
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
    public class AuditLog
    {
        public int code { get; set; }
        public string message { get; set; }
    }
}
