using SparrowPlatform.Domain.Interfaces;
using SparrowPlatform.Infrastruct.Data;

namespace SparrowPlatform.Infrastruct.UoW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SparrowPlatformDbContext _dbContext;

        public UnitOfWork(SparrowPlatformDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool Commit()
        {
            return _dbContext.SaveChanges() > 0;
        }

        //手动回收
        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
