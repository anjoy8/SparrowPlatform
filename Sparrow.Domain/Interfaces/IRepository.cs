using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SparrowPlatform.Domain.Interfaces
{
    public interface IRepository<TEntity> : IDisposable where TEntity : class
    {
        
        void Add(TEntity obj);

        void AddRange(List<TEntity> objArr);

        TEntity GetById(object id);
       
        IQueryable<TEntity> GetAll();
       
        void Update(TEntity obj);
       
        void Remove(object id);
       
        int SaveChanges();

        IQueryable<TEntity> GetPageList(Expression<Func<TEntity, bool>> lambdawhere, RequestPages requestPages);

        void RemoveRange(Expression<Func<TEntity, bool>> lambdawhere);
    }

}
