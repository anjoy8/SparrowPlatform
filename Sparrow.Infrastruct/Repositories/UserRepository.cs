using SparrowPlatform.Domain.Interfaces;
using SparrowPlatform.Domain.Models;
using SparrowPlatform.Infrastruct.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace SparrowPlatform.Infrastruct.Repositories
{
    public class UserRepository : Repository<UserInfo>, IUserRepository
    {
        public UserRepository(SparrowPlatformDbContext context)
          : base(context)
        {

        }

        public UserInfo GetUserInfoByLogin(string login)
        {
            return DbSet.AsNoTracking().FirstOrDefault(c => c.Login == login && c.IsDeleted == false);
        }
    }
}
