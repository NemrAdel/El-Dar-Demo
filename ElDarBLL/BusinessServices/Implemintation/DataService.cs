using AutoMapper;
using ElDarBLL.BusinessServices.Interfaces;
using ElDarBLL.Helpers;
using ElDarBLL.ViewModel.DataVM;
using ElDarDal.Context;
using ElDarDal.Models;
using ElDarDal.UnitOfWork;

namespace ElDarBLL.BusinessServices.Implemintation
{
    public class DataService : IDataService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ElDarDbContext _dbContext;
        private readonly IMapper _mapper;

        public DataService(IUnitOfWork unitOfWork , ElDarDbContext dbContext , IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
            _mapper = mapper;
        }
        public bool AddData(CreateDataVM createData)
        {
            try
            {
                if (createData == null)
                {
                    throw new ArgumentNullException(nameof(createData), "CreateDataVM cannot be null.");
                }

                var dataEntity = _mapper.Map<Data>(createData);

                var nowUtc = DateTime.UtcNow;
                dataEntity.CreatedAt = nowUtc;
                dataEntity.UpdatedAt = nowUtc;

                dataEntity.IsSynced = false;

                if (dataEntity.Date != default)
                {
                    dataEntity.Date = DateTime.SpecifyKind(dataEntity.Date.Date, DateTimeKind.Utc);
                }

                _unitOfWork.GetRepository<Data>().Add(dataEntity);
                return _unitOfWork.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while adding data: {ex.Message}", ex);
            }
        }

        public string ConvertDelete(Guid id)
        {
            var data = _unitOfWork.GetRepository<Data>().GetById(id);
            if (data == null)
            {
                return "Data Not Found";
            }
            if (data.IsDeleted)
                data.IsDeleted = false;
            else
                data.IsDeleted = true;
            data.IsSynced = false;
            data.UpdatedAt = DateTime.UtcNow;
            var isDeleted = _unitOfWork.SaveChanges();
            return "Converted Successfully";
        }

        public bool DeleteData(Guid id)
        {
            try
            {
                var data= _unitOfWork.GetRepository<Data>().GetById(id);
                if (data == null)
                {
                    return false;
                }
                _dbContext.DeletedLogs.Add(new DeletedLogs
                {
                    Id = id,
                    TableName = "Data",
                    DeletedAt= DateTime.UtcNow
                });

                _unitOfWork.GetRepository<Data>().Delete(data); 
                return _unitOfWork.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while deleting data: {ex.Message}", ex);
            }
        }

        public IEnumerable<DataVM> GetAllDataAll()
        {
            var dataList = _unitOfWork.GetRepository<Data>().GetAll().OrderByDescending(d => d.CreatedAt).ToList();
            return _mapper.Map<IEnumerable<DataVM>>(dataList);
        }

        public int GetAllDataCount()
        {
            var count = _unitOfWork.GetRepository<Data>().GetAll(d => !d.IsDeleted).Count();
            return count;
        }

        public List<DataVM> GetDataByDate(DateTime dateTime)
        {
            var filteredData = _unitOfWork.GetRepository<Data>()
                .GetAll(d => d.Date.Date == dateTime.Date && !d.IsDeleted).OrderByDescending(d => d.CreatedAt);

            return _mapper.Map<List<DataVM>>(filteredData);
        }

        public DataVM? GetDataById(Guid id)
        {
            var data = _unitOfWork.GetRepository<Data>().GetById(id);
            return _mapper.Map<DataVM>(data);
        }


        public bool UpdateData(Guid id, UpdateDataVM updateData)
        {
            try
            {
                var data = _unitOfWork.GetRepository<Data>().GetById(id);
                if (data == null)
                {
                    return false;
                }
                _mapper.Map(updateData, data);
                data.UpdatedAt = DateTime.UtcNow;
                data.IsSynced = false;
                return _unitOfWork.SaveChanges() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating data: {ex.Message}", ex);
            }
        }

        public PaginatedList<DataVM> GetPagedData(int pageNumber = 1, int pageSize = 60)
        {
            var query = _unitOfWork.GetRepository<Data>().GetAll().OrderByDescending(d => d.CreatedAt);
            var totalCount = query.Count();
            var items = query.Skip((pageNumber - 1) * pageSize)
                             .Take(pageSize)
                             .ToList();
            var mappedItems = _mapper.Map<List<DataVM>>(items);
            return new PaginatedList<DataVM>(mappedItems, totalCount, pageNumber, pageSize);
        }
    }
}
