using AutoMapper;
using ElDarBLL.BusinessServices.Interfaces;
using ElDarBLL.Helpers;
using ElDarBLL.ViewModel.EmployeeVM;
using ElDarDal.Context;
using ElDarDal.Models;
using ElDarDal.Models.Enums;
using ElDarDal.UnitOfWork;

namespace ElDarBLL.BusinessServices.Implemintation
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ElDarDbContext _dbContext;
        private readonly IMapper _mapper;

        public EmployeeService
            (
            IUnitOfWork unitOfWork , 
            ElDarDbContext dbContext,
            IMapper mapper
            )
        {
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public bool AddEmployee(CreateEmployeeVM employee)
        {
            var PhoneNumberExists = _dbContext.Employees.Any(e => e.PhoneNumber == employee.PhoneNumber);
            if (PhoneNumberExists)
                return false;
            
            var mappedEmployee = _mapper.Map<CreateEmployeeVM, Employee>(employee);
            var now = DateTime.UtcNow;
            mappedEmployee.CreatedAt = now;
            mappedEmployee.UpdatedAt = now;
            mappedEmployee.IsSynced = false;

            _unitOfWork.GetRepository<Employee>().Add(mappedEmployee);
            var is_Created = _unitOfWork.SaveChanges();

            return is_Created > 0;
        }

        public bool DeleteEmployee(Guid id)
        {
            try
            {
                var employee = _unitOfWork.GetRepository<Employee>().GetById(id);
                if (employee == null)
                {
                    return false;
                }
                _dbContext.DeletedLogs.Add(new DeletedLogs
                {
                    Id = id,
                    TableName = "Employees",
                });
                _unitOfWork.GetRepository<Employee>().Delete(employee);
                var is_Deleted = _unitOfWork.SaveChanges();
                return is_Deleted > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting employee: {ex.Message}");
                return false;
            }
        }

        public IEnumerable<EmployeeVM> GetAllEmployees()
        {
            var employees = _unitOfWork.GetRepository<Employee>().GetAll(e=>!e.IsDeleted && e.Status == Status.Active);
            if (employees is null || !employees.Any()) return [];
            return _mapper.Map<IEnumerable<Employee>, IEnumerable<EmployeeVM>>(employees);
        }

        public EmployeeVM? GetEmployeeById(Guid id)
        {
            var employee = _unitOfWork.GetRepository<Employee>().GetById(id);
            if (employee == null) return null;

            return _mapper.Map<Employee, EmployeeVM>(employee);
        }

        public Guid GetEmployeeIdByPhoneNumber(string phone)
        {
            var employee = _unitOfWork.GetRepository<Employee>().GetAll(e => e.PhoneNumber == phone).FirstOrDefault();
            if (employee == null)
            {
                return Guid.Empty;
            }
            return employee.Id;
        }

        public EmployeeVM? GetEmployeeByPhoneNumber(string phone)
        {
            var employee = _unitOfWork.GetRepository<Employee>().GetAll(e => e.PhoneNumber == phone).FirstOrDefault();
            if (employee == null)
            {
                return null;
            }
            return _mapper.Map<Employee, EmployeeVM>(employee);
        }

        public bool MarkEmployeeAsPaid(Guid id)
        {
            var employee = _unitOfWork.GetRepository<Employee>().GetById(id);
            if (employee == null)
            {
                return false;
            }
            employee.IsPaid = true;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.IsSynced = false;
            _unitOfWork.GetRepository<Employee>().Update(employee);

            var isUpdated = _unitOfWork.SaveChanges();
            return isUpdated > 0;
        }

        public bool UpdateEmployee(Guid id,UpdateEmployeeVM employee)
        {
            var existingEmployee = _unitOfWork.GetRepository<Employee>().GetById(id);
            if (existingEmployee == null)
            {
                return false;
            }

            var mappedEmployee = _mapper.Map(employee, existingEmployee);
            mappedEmployee.UpdatedAt= DateTime.UtcNow;
            mappedEmployee.IsSynced = false;
            _unitOfWork.GetRepository<Employee>().Update(mappedEmployee);
            var isUpdated = _unitOfWork.SaveChanges();
            return isUpdated > 0;
        }

        public IEnumerable<EmployeeVM> GetAllEmployeesAll()
        {
            var employees = _unitOfWork.GetRepository<Employee>().GetAll();
            if (employees is null || !employees.Any()) return [];
            return _mapper.Map<IEnumerable<Employee>, IEnumerable<EmployeeVM>>(employees);
        }

        public EmployeeVM GetEmployeeCard(string CardNumber)
        {
            var employee = _dbContext.Employees.FirstOrDefault(e => e.CardNumber == CardNumber);
            if (employee == null)
                return null!;
            return _mapper.Map<EmployeeVM>(employee);
        }

        public string ConvertDelete(Guid id)
        {
            var employee = _unitOfWork.GetRepository<Employee>().GetById(id);
            if (employee == null)
            {
                return "Employee Not Found";
            }
            if(employee.IsDeleted)
                employee.IsDeleted = false;
            else
                employee.IsDeleted = true;

            employee.IsSynced = false;
            employee.UpdatedAt = DateTime.UtcNow;
            var isDeleted = _unitOfWork.SaveChanges();
            return "Converted Successfully";
        }
        public PaginatedList<EmployeeVM> GetPagedData(int pageNumber = 1, int pageSize = 15 , bool includeDeleted = false , bool isActive = true)
        {
            var query = includeDeleted 
                ? _unitOfWork.GetRepository<Employee>().GetAll().OrderByDescending(e => e.CreatedAt)
                : _unitOfWork.GetRepository<Employee>().GetAll(e=>!e.IsDeleted && e.Status == Status.Active).OrderByDescending(e => e.CreatedAt);
            var totalCount = query.Count();
            var items = query.Skip((pageNumber - 1) * pageSize)
                             .Take(pageSize)
                             .ToList();
            var mappedItems = _mapper.Map<List<EmployeeVM>>(items);
            return new PaginatedList<EmployeeVM>(mappedItems, totalCount, pageNumber, pageSize);
        }
    }
}
