using ElDarDal.Models;
using ElDarDal.Repositories.Interfaces;

namespace ElDarDal.UnitOfWork
{
    public interface IUnitOfWork
    {
        IGenericRepository<T>GetRepository<T>() where T : BaseEntity, new();
        int SaveChanges();
    }
}
