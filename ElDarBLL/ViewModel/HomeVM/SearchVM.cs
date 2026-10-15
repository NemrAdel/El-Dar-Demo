using ElDarBLL.Helpers;
using ElDarDal.Models;

namespace ElDarBLL.ViewModel.HomeVM
{

    public class SearchViewModel
    {
        public PaginatedList<Guest> Guests { get; set; } 
        public PaginatedList<Employee> Employees { get; set; } 
        public PaginatedList<Data> DataList { get; set; }
    }
    
}
