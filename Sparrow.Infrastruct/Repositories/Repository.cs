using SparrowPlatform.Domain.Interfaces;
using SparrowPlatform.Infrastruct.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace SparrowPlatform.Infrastruct.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly SparrowPlatformDbContext Db;
        protected readonly DbSet<TEntity> DbSet;

        public Repository(SparrowPlatformDbContext context)
        {
            Db = context;
            DbSet = Db.Set<TEntity>();
        }

        public virtual void Add(TEntity obj)
        {
            DbSet.Add(obj);
        }

        public virtual void AddRange(List<TEntity> objArr)
        {
            DbSet.AddRange(objArr);
        }

        public virtual TEntity GetById(object id)
        {
            return DbSet.Find(id);
        }

        public virtual IQueryable<TEntity> GetAll()
        {
            return DbSet;
        }

        public virtual IQueryable<TEntity> GetPageList(Expression<Func<TEntity, bool>> lambdawhere, RequestPages requestPages)
        {
            requestPages.Page = requestPages.Page > 0 ? requestPages.Page : 1;
            requestPages.TotalCount = DbSet.Where(lambdawhere).Count();

            //return Db.Set<TEntity>().Where(lambdawhere)
            //    .OrderBy(requestPages.Sorting)
            //    .Skip((requestPages.Page - 1) * requestPages.PageSize)
            //    .Take(requestPages.PageSize);

            //条件过滤
            var query = Db.Set<TEntity>().Where(lambdawhere);

            //创建表达式变量参数
            var parameter = Expression.Parameter(typeof(TEntity), "o");

            //根据属性名获取属性
            var property = typeof(TEntity).GetProperty(requestPages.Sorting);
            //创建一个访问属性的表达式
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            string OrderName = requestPages.IsDesc ? "OrderByDescending" : "OrderBy";
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), OrderName, new Type[] { typeof(TEntity), property.PropertyType }, query.Expression, Expression.Quote(orderByExp));

            query = query.Provider.CreateQuery<TEntity>(resultExp);
            return query.Skip((requestPages.Page - 1) * requestPages.PageSize).Take(requestPages.PageSize);
        }


        public virtual void Update(TEntity obj)
        {
            DbSet.Update(obj);
        }

        public virtual void Remove(object id)
        {
            DbSet.Remove(DbSet.Find(id));
        }

        public virtual void RemoveRange(Expression<Func<TEntity, bool>> lambdawhere)
        {
            var removeItems = Db.Set<TEntity>().Where(lambdawhere);
            DbSet.RemoveRange(removeItems);
        }

        public int SaveChanges()
        {
            return Db.SaveChanges();
        }

        public void Dispose()
        {
            Db.Dispose();
            GC.SuppressFinalize(this);
        }
    }

}
