using ElDarDal.Models;

namespace ElDarDal.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity , new()
    {
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        T? GetById(Guid id);
        IEnumerable<T> GetAll(Func<T,bool>? condition=null);
    }
}
