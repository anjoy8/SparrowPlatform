using SparrowPlatform.Application.ViewModels;
using SparrowPlatform.Domain.Interfaces;
using SparrowPlatform.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SparrowPlatform.Application.Services
{
    /// <summary>
    /// the interface of user service.
    /// </summary>
    public interface IUserService
    {
        List<UserResponse> GetAll(Expression<Func<UserInfo, bool>> whereExpression = null);
        ApiResultVo<UserResponse> AddOne(UserVo userVo);
        ApiResultVo<UserResponse> Update(UserUpdateVo userVo);
        ApiResultVo<UserResponse> Delete(int Id);
        List<UserResponse> GetPageList(RequestPages requestPages, Expression<Func<UserInfo, bool>> whereExpression = null);
        ApiResultVo<UserResponse> GetOne(int id);
    }
}
