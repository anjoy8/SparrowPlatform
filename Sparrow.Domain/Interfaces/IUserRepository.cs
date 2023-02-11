using SparrowPlatform.Domain.Models;

namespace SparrowPlatform.Domain.Interfaces
{
    public interface IUserRepository : IRepository<UserInfo>
    {
        UserInfo GetUserInfoByLogin(string login);
    }
}
