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
        private readonly IUserService _userService;

        public UserController(ILogger<UserController> logger,
            IHttpContextAccessor accessor,
            IUser user,
            IUserService userService)
        {
            _logger = logger;
            _accessor = accessor;
            _user = user;
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


    }
}
