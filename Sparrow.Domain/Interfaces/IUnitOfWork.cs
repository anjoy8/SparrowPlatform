using System;

namespace SparrowPlatform.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        bool Commit();
    }
}
