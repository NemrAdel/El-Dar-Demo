using ElDarDal.Context;
using ElDarDal.Models;
using ElDarDal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace ElDarDal.Repositories.Implemintation
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity, new()
    {
        private readonly ElDarDbContext _dbContext;

        public GenericRepository(ElDarDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public void Add(T entity)
        {
            _dbContext.Set<T>().Add(entity);
        }

        public void Update(T entity)
        {
            _dbContext.Set<T>().Update(entity);
        }

        public void Delete(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
        }

        public T? GetById(Guid id)
        {
            return _dbContext.Set<T>().Find(id);
        }

        public IEnumerable<T> GetAll(Func<T, bool>? condition = null)
        {
            if (condition == null)
            {
                return _dbContext.Set<T>().AsNoTracking().ToList();
            }
            return _dbContext.Set<T>().AsNoTracking().Where(condition).ToList();
        }
    }
}