using SparrowPlatform.API.Utils;
using SparrowPlatform.Application.Services;
using SparrowPlatform.Application.ViewModels;
using SparrowPlatform.Domain.Interfaces;
using SparrowPlatform.Domain.Models;
using SparrowPlatform.Infrastruct.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SparrowPlatform.API.Controllers
{
    /// <summary>
    /// User manager
    /// </summary>
    [ApiController]
    [Route("api/spa/[controller]")]
    [Authorize(Policy = Authorizor.OnlyCanReadDemo)]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly IUser _user;
        private readonly IRoleInfoService _roleInfoService;
        private readonly IApplicationService _applicationInfoService;
        private readonly IUserService _userService;

        public UserController(ILogger<UserController> logger,
            IHttpContextAccessor accessor,
            IUser user,
            IRoleInfoService roleInfoService,
            IApplicationService applicationInfoService,
            IUserService userService)
        {
            _logger = logger;
            _accessor = accessor;
            this._user = user;
            this._roleInfoService = roleInfoService;
            this._applicationInfoService = applicationInfoService;
            _userService = userService;
        }

        /// <summary>
        /// Get all User information and support paging.
        /// </summary>
        /// <param name="requestPages"></param>
        /// <returns></returns>
        [HttpGet]
        public object Get([FromQuery] UserRequest requestPages)
        {
            Expression<Func<UserInfo, bool>> whereExpression = d =>
                d.IsDeleted == false &&
                (requestPages.Name.ObjToString() != "" ? (d.DisplayName.Contains(requestPages.Name) || d.Login.Contains(requestPages.Name)) : true);

            if (requestPages.Page == -1)
            {
                return ApiResultVo<ResponseModelBase<UserResponse>>.ok(new ResponseModelBase<UserResponse>()
                {
                    data = _userService.GetAll(whereExpression),
                });
            }

            return ApiResultVo<PageModel<UserResponse>>.ok(new PageModel<UserResponse>()
            {
                data = _userService.GetPageList(requestPages, whereExpression),
                Page = requestPages.Page,
                PageSize = requestPages.PageSize,
                TotalCount = requestPages.TotalCount,
            });
        }

        /// <summary>
        /// Get details of a User.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public ApiResultVo<UserResponse> Detail(int id)
        {
            return _userService.GetOne(id);
        }

        /// <summary>
        /// Get details of a User by token.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("my")]
        public ApiResultVo<UserResponse> My()
        {
            var response = new ApiResultVo<UserResponse>();

            var userClaim = User.Claims?.FirstOrDefault(c => c.Type == "sub");
            if (userClaim != null && !string.IsNullOrEmpty(userClaim.Value))
            {
                return _userService.GetOneByAAD(userClaim.Value);
            }

            return response;
        }

        /// <summary>
        /// Add a User record.
        /// </summary>
        /// <param name="userVo"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResultVo<UserResponse> Add(UserVo userVo)
        {
            //userVo.Id = TxtTool.GetNextSnowId();
            userVo.EditAt = DateTime.Now.AddHours(8);
            if (userVo.EditBy.ObjToString() == "")
            {
                userVo.EditBy = _user.GetName();
            }
            userVo.CreatedAt = userVo.EditAt;
            userVo.CreatedBy = userVo.EditBy;
            return _userService.AddOne(userVo);
        }

        /// <summary>
        /// Update a User record.
        /// </summary>
        /// <param name="userVo"></param>
        /// <returns></returns>
        [HttpPut]
        public ApiResultVo<UserResponse> Update(UserUpdateVo userVo)
        {
            userVo.EditAt = DateTime.Now.AddHours(8);
            if (userVo.EditBy.ObjToString() == "")
            {
                userVo.EditBy = _user.GetName();
            }
            return _userService.Update(userVo);
        }

        /// <summary>
        /// Delete a User record.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public ApiResultVo<UserResponse> Delete(int id)
        {
            return _userService.Delete(id);
        }


        [HttpGet]
        [Route("valid")]
        public ApiResultVo<string> Valid(string key, int status)
        {
            return _userService.ValidUser(key, status);
        }


        [HttpGet]
        [Route("query")]
        public ApiResultVo<ResponseModelBase<UserResponse>> Query()
        {
            try
            {
                var application = (Request?.Headers["X-Application"])?.ObjToString();
                if (!application.IsNotEmptyOrNull())
                {
                    return ApiResultVo<ResponseModelBase<UserResponse>>.error("参数不能为空");
                }

                var allUser = _userService.GetAll(d => d.IsDeleted == false);
                var allRole = _roleInfoService.GetAll(d => d.IsDeleted == false);
                var allApplication = _applicationInfoService.GetAll(d => d.IsDeleted == false);
                var allRoleApp = _userService.GetAllRoleApplications();

                var applicationId = (allApplication.FirstOrDefault(d => d.Name == application)?.Id ?? "0").ObjToLong();

                if (!(applicationId > 0))
                {
                    return ApiResultVo<ResponseModelBase<UserResponse>>.error("该Application不存在");
                }

                List<UserResponse> rltUsers = new();

                if (allRole?.Any() == true)
                {
                    foreach (var item in allUser)
                    {
                        if (item?.Role != null && item?.Role?.id > 0)
                        {
                            var roleModel = allRole.FirstOrDefault(d => d.Id == item.Role.id);
                            if (roleModel != null)
                            {
                                var accessApplications = allRoleApp.Where(d => d.RoleInfoId == roleModel.Id && d.ApplicationInfoId == applicationId);
                                try
                                {
                                    if (roleModel.fullAccessToApplications || roleModel.ApplicationScopeAll?.Contains(application) == true || accessApplications.Any())
                                    {
                                        rltUsers.Add(item);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("add user error:" + e.Message + e.StackTrace);
                                }
                            }
                        }
                    }
                }

                Console.WriteLine("query user success");
                return ApiResultVo<ResponseModelBase<UserResponse>>.ok(new ResponseModelBase<UserResponse>()
                {
                    data = rltUsers,
                });

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);
                return ApiResultVo<ResponseModelBase<UserResponse>>.error("查询失败，服务器异常");
            }

        }

        [HttpGet]
        [Route("applications")]
        public ApiResultVo<ResponseModelBase<UserApplicationResponse>> Applications()
        {
            try
            {
                var userAadid = _user.GetAADID();
                if (!userAadid.IsNotEmptyOrNull())
                {
                    return ApiResultVo<ResponseModelBase<UserApplicationResponse>>.error("用户未登录");
                }

                var currentUser = _userService.GetAll(d => d.IsDeleted == false && d.AADId == userAadid).FirstOrDefault();
                if (!(currentUser != null && currentUser.Id > 0 && currentUser.Role.id > 0))
                {
                    return ApiResultVo<ResponseModelBase<UserApplicationResponse>>.error("用户数据查询失败");
                }
                var userRoleModel = _roleInfoService.GetById(currentUser.Role.id);
                var allApplication = _applicationInfoService.GetAll(d => d.IsDeleted == false);
                if (allApplication == null)
                {
                    return ApiResultVo<ResponseModelBase<UserApplicationResponse>>.error("应用数据异常");
                }

                List<string> filterApplications = new();
                // 非全量，从关系表中获取
                if (!userRoleModel.fullAccessToApplications)
                {
                    filterApplications = _applicationInfoService.GetApplicationsByRoleId(currentUser.Role.id);

                    if (userRoleModel.ApplicationScopeAll.IsNotEmptyOrNull())
                    {
                        var filterApplications2 = userRoleModel.ApplicationScopeAll.Split(",").ToList();
                        filterApplications.AddRange(filterApplications2);
                    }
                }
                else
                {
                    filterApplications = allApplication.Select(d => d.Name).ToList();
                }

                var userApplications = new List<UserApplicationResponse>();
                foreach (var item in allApplication)
                {
                    if (filterApplications.Contains(item.Name))
                    {
                        userApplications.Add(new UserApplicationResponse()
                        {
                            Name = item.Name,
                            Description = item.Description,
                            FullName = item.FullName,
                        });
                    }
                }

                Console.WriteLine("applications user success");
                return ApiResultVo<ResponseModelBase<UserApplicationResponse>>.ok(new ResponseModelBase<UserApplicationResponse>()
                {
                    data = userApplications
                });

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);
                return ApiResultVo<ResponseModelBase<UserApplicationResponse>>.error("查询失败，服务器异常");
            }

        }


        [HttpGet]
        [Route("aad")]
        public UserResponse DetailByAadid(string aadid)
        {
            try
            {
                if (!aadid.IsNotEmptyOrNull())
                {
                    return new UserResponse();
                }
                return (_userService.GetOneByAAD(aadid))?.response ?? new UserResponse();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);
                return new UserResponse();
            }
        }
    }
}
