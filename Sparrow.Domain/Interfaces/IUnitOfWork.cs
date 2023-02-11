using System;

namespace SparrowPlatform.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        //是否提交成功
        bool Commit();
    }
}
