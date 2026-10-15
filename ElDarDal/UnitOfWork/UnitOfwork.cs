using ElDarDal.Context;
using ElDarDal.Models;
using ElDarDal.Repositories.Implemintation;
using ElDarDal.Repositories.Interfaces;

namespace ElDarDal.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ElDarDbContext _dbContext;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(ElDarDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IGenericRepository<T> GetRepository<T>() where T : BaseEntity, new()
        {
            var entityType = typeof(T);

            if(_repositories.TryGetValue(entityType,out var repository))
                return (IGenericRepository<T>)repository;

            var newRepository = new GenericRepository<T>(_dbContext);

            _repositories[entityType] = newRepository;
            return newRepository;
        }

        public int SaveChanges()
        {
            return _dbContext.SaveChanges();
        }
    }
}
