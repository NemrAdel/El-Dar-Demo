using ElDarBLL.BusinessServices.Interfaces;
using ElDarBLL.ViewModel.DataVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace El_Dar.Controllers
{
    public class DataController : Controller
    {
        private readonly IDataService _dataservice;

        public DataController(IDataService dataservice) 
        {
            _dataservice = dataservice;
        }
        [Authorize]
        [HttpGet]
        public IActionResult Index(DateTime date)
        {
            var data = _dataservice.GetDataByDate(date);
            ViewBag.SelectedDate = date.ToString("yyyy-MM-dd");
            return View(data);
        }
        [Authorize]
        [HttpGet]
        public IActionResult GetDataByDate(DateTime date)
        {
            var filteredData = _dataservice.GetDataByDate(date);
            ViewBag.SelectedDate = date.ToString("yyyy-MM-dd");
            return View("Index", filteredData);
        }
        [Authorize]
        [HttpGet]
        public IActionResult GetAllData(int pageNumber = 1 , int pageSize = 50)
        {
            var data = _dataservice.GetPagedData(pageNumber, pageSize);
            return View(data);
        }
        [Authorize]
        [HttpPost]
        public IActionResult CreateData([FromForm] CreateDataVM createData)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("GetDataByDate",new {date=createData.Date});
            }
            _dataservice.AddData(createData);
            return RedirectToAction("GetDataByDate", new { date = createData.Date });
        }
        [Authorize]
        [HttpPost]
        public IActionResult DeleteData([FromQuery] Guid id)
        {
            var data= _dataservice.GetDataById(id);
            if (data == null)
            {
                return NotFound();
            }
            var deleted = _dataservice.DeleteData(id);
            string? refererUrl = Request.Headers["Referer"].ToString();
            return Redirect(refererUrl);
  
        }
        [Authorize]
        [HttpGet]
        public IActionResult UpdateDataView([FromQuery] Guid id, string returnUrl = null!)
        {
            var data = _dataservice.GetDataById(id);
            if (data == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(returnUrl))
            {
                string referer = Request.Headers["Referer"].ToString();

                if (!string.IsNullOrEmpty(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var uri))
                {
                    returnUrl = uri.PathAndQuery;
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            Console.WriteLine($"ReturnUrl ---------------------------------------------: {returnUrl}");

            return View(data);
        }
        [Authorize]
        [HttpPost]
        public IActionResult UpdateData([FromQuery] Guid id , UpdateDataVM updateDataVM, string returnUrl = null!)
        {
            var data= _dataservice.GetDataById(id);
            if (data == null)
            {
                return NotFound();
            }
            var updatedData = _dataservice.UpdateData(id, updateDataVM);

            if(updatedData)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectToAction("Index", new { date = data.Date });
            }
            return RedirectToAction("UpdateDataView", data);
        }
        [Authorize]
        [HttpPost]
        public IActionResult ConvertDelete([FromQuery] Guid id)
        {

            if (id == Guid.Empty)
            {
                return RedirectToAction("GetAllData");
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
