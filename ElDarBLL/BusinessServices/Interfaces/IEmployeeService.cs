using ElDarBLL.Helpers;
using ElDarBLL.ViewModel.EmployeeVM;

namespace ElDarBLL.BusinessServices.Interfaces
{
    public interface IEmployeeService
    {
        IEnumerable<EmployeeVM> GetAllEmployees();
        IEnumerable<EmployeeVM> GetAllEmployeesAll();
        EmployeeVM? GetEmployeeById(Guid id);
        Guid GetEmployeeIdByPhoneNumber(string phone);
        EmployeeVM GetEmployeeCard(string CardNumber);
        EmployeeVM? GetEmployeeByPhoneNumber(string phone);
        bool AddEmployee(CreateEmployeeVM employee);
        bool UpdateEmployee(Guid id,UpdateEmployeeVM employee);
        bool DeleteEmployee(Guid id);
        string ConvertDelete(Guid id);
        bool MarkEmployeeAsPaid(Guid id);
        PaginatedList<EmployeeVM> GetPagedData(int pageNumber = 1, int pageSize = 15 , bool includeDeleted = false , bool isActive = true);

    }
}
