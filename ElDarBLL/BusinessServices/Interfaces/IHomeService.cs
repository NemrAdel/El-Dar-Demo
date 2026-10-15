
using ElDarBLL.ViewModel.DataVM;
using ElDarBLL.ViewModel.HomeVM;
using ElDarDal.Models.Enums;

namespace ElDarBLL.BusinessServices.Interfaces
{
    public interface IHomeService
    {
        SearchViewModel SearchCriteria(
            string searchTarget,
            string searchBy,
            string searchQuery,
            DateTime? searchDate,
            string searchStatus,
            JobEnum? searchJob,
            Revenues? searchRevenue,
            Payments? searchPayment,
            int pageNumber = 1,
            int pageSize = 50);

        List<string> GetMonthDate(DateTime? selectedDate);
        IEnumerable<DataVM> DataMonthely(DateTime? selectedDate);
        List<string> GetYearDate(DateTime? selectedDate);
        IEnumerable<DataVM> DataYearly(DateTime? selectedDate);
    }
}
