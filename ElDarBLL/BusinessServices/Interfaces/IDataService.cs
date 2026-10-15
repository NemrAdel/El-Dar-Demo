using ElDarBLL.Helpers;
using ElDarBLL.ViewModel.DataVM;

namespace ElDarBLL.BusinessServices.Interfaces
{
    public interface IDataService
    {
        //IEnumerable<DataVM> GetAllData(DateTime dateTime);
        IEnumerable<DataVM> GetAllDataAll();
        List<DataVM> GetDataByDate(DateTime dateTime);
        DataVM? GetDataById(Guid id);
        bool AddData(CreateDataVM createData);
        bool UpdateData(Guid id, UpdateDataVM updateData);
        bool DeleteData(Guid id);
        string ConvertDelete(Guid id);
        int GetAllDataCount();
        PaginatedList<DataVM> GetPagedData(int pageNumber = 1, int pageSize = 60);
    }
}
