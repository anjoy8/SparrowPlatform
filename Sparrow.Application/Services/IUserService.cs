using SparrowPlatform.Application.ViewModels;
using SparrowPlatform.Domain.Interfaces;
using SparrowPlatform.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SparrowPlatform.Application.Services
{
    /// <summary>
    /// Interface of user service.
    /// </summary>
    public interface IUserService
    {
        List<UserResponse> GetAll(Expression<Func<UserInfo, bool>> whereExpression = null);
        ApiResultVo<UserResponse> AddOne(UserVo userVo);
        ApiResultVo<UserResponse> Update(UserUpdateVo userVo);
        ApiResultVo<UserResponse> Delete(int Id);
        List<UserResponse> GetPageList(RequestPages requestPages, Expression<Func<UserInfo, bool>> whereExpression = null);
        ApiResultVo<UserResponse> GetOne(int id);
        ApiResultVo<UserResponse> GetOneByAAD(string aadid = "");
        ApiResultVo<string> ValidUser(string key = "", int status = 0);
        List<UserStatusAlertVo> QueryUserExpired();
        List<UserStatusAlertVo> QueryUserExpiredToAdmin();
        bool emailAuditLog(string email = "", string htmlEmail = "", string functionDef = "");
        bool getCountApplicationinfoByRoleid(int id, string appName);
        List<RoleApplication> GetAllRoleApplications();
    }
}
