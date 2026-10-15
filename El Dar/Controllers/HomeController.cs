using ElDarBLL.BusinessServices.Interfaces;
using ElDarBLL.ViewModel.DataVM;
using ElDarDal.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace El_Dar.Controllers
{
    public class HomeController : Controller
    {
        private readonly IGuestService _guestService;
        private readonly IEmployeeService _employeeService;
        private readonly IHomeService _homeService;
        private readonly IDataService _dataservice;

        public HomeController(
            IGuestService guestService ,
            IEmployeeService employeeService ,
            IHomeService homeService,
            IDataService dataservice

            )
        {
            _guestService = guestService;
            _employeeService = employeeService;
            _homeService = homeService;
            _dataservice = dataservice;
        }
        [Authorize]
        [HttpGet]
        public IActionResult Home()
        {
            List<int> Detatils = new List<int>();
            var GuestNumber= _guestService.GetAllGuests().Count();
            var AllGuestNumber= _guestService.GetAllGuestsAll().Count();
            var EmployeeNumber= _employeeService.GetAllEmployees().Count();
            var AllEmployeeNumber= _employeeService.GetAllEmployeesAll().Count();
            var AllDataNumber= _dataservice.GetAllDataCount();
            var AllDataNumberAll= _dataservice.GetAllDataAll().Count();
            Detatils.Add(GuestNumber);
            Detatils.Add(AllGuestNumber);
            Detatils.Add(EmployeeNumber);
            Detatils.Add(AllEmployeeNumber);
            Detatils.Add(AllDataNumber);
            Detatils.Add(AllDataNumberAll);
            ViewBag.Details = Detatils;
            return View();
        }
        [Authorize]
        [HttpGet]
        public IActionResult Search()
        {
            return View();
        }
        [Authorize]
        [HttpGet]
        public IActionResult SearchData(
            string searchTarget,
            string searchBy,
            string searchQuery,
            DateTime? searchDate,
            string searchStatus, 
            JobEnum? searchJob,
            Revenues? searchRevenue,
            Payments? searchPayment,
            int pageNumber = 1,
            int pageSize = 50)
        {
            bool hasSearchValues = !string.IsNullOrEmpty(searchTarget)
                        || !string.IsNullOrEmpty(searchQuery)
                        || searchDate.HasValue
                        || searchJob.HasValue
                        || searchRevenue.HasValue
                        || searchPayment.HasValue;

            if (!hasSearchValues)
            {
                return View();
            }
            var results = _homeService.SearchCriteria(
                searchTarget,
                searchBy,
                searchQuery,
                searchDate,
                searchStatus,
                searchJob,
                searchRevenue,
                searchPayment,
                pageNumber,
                pageSize
            );
            return View(results);
        }
        [Authorize]
        [HttpGet]
        public IActionResult Month(DateTime? date)
        {
            var ListOfDates = _homeService.GetMonthDate(date);
            ViewBag.ListOfDates = ListOfDates;
            var Data = _homeService.DataMonthely(date);
            return View(Data);
        }
        [Authorize]
        [HttpGet]
        public IActionResult Year(DateTime? date)
        {
            var ListOfDates = _homeService.GetYearDate(date);
            ViewBag.ListOfDates = ListOfDates;
            var Data = _homeService.DataYearly(date);
            return View(Data);
        }
        [Authorize]
        [HttpPost]
        public IActionResult ConvertDelete([FromQuery] Guid id)
        {

            if (id == Guid.Empty)
            {
                return Json(new { success = false, message = "Must Enter The Id Or Phone To Delete ❌" });
            }
            DataVM? data = null;
            data = _dataservice.GetDataById(id);

            var DataName = data!.Name ?? "Unknown";
            var deleted = _dataservice.ConvertDelete(id);
            string? refererUrl = Request.Headers["Referer"].ToString();
            return Redirect(refererUrl);

        }
    }
}
