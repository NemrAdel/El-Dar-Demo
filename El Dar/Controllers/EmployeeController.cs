using ElDarBLL.BusinessServices.Interfaces;
using ElDarBLL.ViewModel.EmployeeVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace El_Dar.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }
        [Authorize]
        [HttpGet]
        public IActionResult Index(int pageNumber = 1, int pageSize = 30 , bool includeDeleted = false, bool isActive = true)
        {
            var employees = _employeeService.GetPagedData(pageNumber, pageSize, includeDeleted, isActive);
            return View(employees);
        }
        [Authorize]
        [HttpGet]
        public IActionResult GetAllEmployees(int pageNumber = 1, int pageSize = 50, bool includeDeleted = true)
        {
            var employees = _employeeService.GetPagedData(pageNumber, pageSize, includeDeleted);
            return View(employees);
        }
        [Authorize]
        [HttpPost]
        public IActionResult CreateEmployee([FromForm]CreateEmployeeVM createEmployee)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var employee = _employeeService.GetEmployeeByPhoneNumber(createEmployee.PhoneNumber);
            var employeeCard = _employeeService.GetEmployeeCard(createEmployee.CardNumber);

            if (employeeCard != null || employee != null)
            {
                ViewBag.PhoneAndCardErrorMessage = $"Employee With Phone Number {createEmployee.PhoneNumber} Or Card Number {createEmployee.CardNumber} Already Exists With {employee.Name} ❌";
                return RedirectToAction("Index", _employeeService.GetPagedData());  
            }
            
            var IsCreated = _employeeService.AddEmployee(createEmployee);
            if (!IsCreated)
            {
                ViewBag.FailedToAddEmployeeMessage = $"Failed To Add Employee With Phone Number {createEmployee.PhoneNumber} ❌";
                return RedirectToAction("Index", _employeeService.GetPagedData());
            }
            return RedirectToAction("Index", _employeeService.GetPagedData());
        }
        [Authorize]
        [HttpGet]
        public IActionResult UpdateEmployeeView([FromQuery] Guid? id, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                string referer = Request.Headers["Referer"].ToString();

                if (!string.IsNullOrEmpty(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var uri))
                {
                    returnUrl = uri.PathAndQuery;
                }
            }

            ViewBag.ReturnUrl = returnUrl;
            var employee = _employeeService.GetEmployeeById(id.Value);
            return View("UpdateEmployee", employee);
        }

        [Authorize]
        [HttpPost]
        public IActionResult UpdateEmployee([FromForm]UpdateEmployeeVM updateEmployee, string returnUrl = null)
        {

            if (!ModelState.IsValid)
            {
                return RedirectToAction("UpdateEmployeeView", new { id = updateEmployee.Id, returnUrl = returnUrl });
            }

            var employee = _employeeService.GetEmployeeByPhoneNumber(updateEmployee.PhoneNumber ?? "");
            if (employee != null && employee.Id != updateEmployee.Id)
            {
                return RedirectToAction("UpdateEmployeeView", new { id = updateEmployee.Id, returnUrl = returnUrl });
            }
            var employeeCard = _employeeService.GetEmployeeCard(updateEmployee.CardNumber ?? "");

            if (employeeCard != null && employeeCard.Id != updateEmployee.Id)
            {
                return RedirectToAction("UpdateEmployeeView", new { id = updateEmployee.Id, returnUrl = returnUrl });
            }
            var idToUpdate =_employeeService.GetEmployeeIdByPhoneNumber(updateEmployee.PhoneNumber ?? "");

            var isUpdated = _employeeService.UpdateEmployee(idToUpdate, updateEmployee);

            if (!isUpdated)
            {
                return RedirectToAction("UpdateEmployeeView", new { id = updateEmployee.Id, returnUrl = returnUrl });
            }
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return Redirect(returnUrl);
        }
        [Authorize]
        [HttpPut]
        public IActionResult PaidEmployee([FromQuery] Guid? id , [FromQuery] string phone)
        {
            if(id==null && string.IsNullOrEmpty(phone))
                return Json(new { success = false, message = "Must Enter The Id Or Phone To Update ❌" });

            EmployeeVM? employee = null;
            if (id.HasValue)
            {
                employee = _employeeService.GetEmployeeById(id.Value);
            }
            else if (!string.IsNullOrEmpty(phone))
            {
                employee = _employeeService.GetEmployeeByPhoneNumber(phone);
            }
            var idToUpdate = id ?? _employeeService.GetEmployeeIdByPhoneNumber(phone!);

            if (employee == null)
            {
                return Json(new {success=false , message= "Employee Not Found ❌", data= (object)null! });
            }
            var isUpdated = _employeeService.MarkEmployeeAsPaid(idToUpdate);

            if (!isUpdated)
            {
                return Json(new {success=false , message= $"Failed to Update Employee: {employee.Name} ❌", data= (object)null! });
            }

            var updatedEmployee = _employeeService.GetEmployeeById(idToUpdate);

            return Json(new {success=true , message= $"Employee: {employee.Name} Has Updated As Paid ✅", data=updatedEmployee });
        }
        [Authorize]
        [HttpPost]
        public IActionResult DeleteEmployee([FromQuery] Guid? id , [FromQuery] string? phone)
        {

            if (id == null && string.IsNullOrEmpty(phone))
                return RedirectToAction("Index");
            
            EmployeeVM? employee = null;
            if (id.HasValue)
                employee = _employeeService.GetEmployeeById(id.Value);
            else if (!string.IsNullOrEmpty(phone))
                employee = _employeeService.GetEmployeeByPhoneNumber(phone);

            var idToDelete = id ?? _employeeService.GetEmployeeIdByPhoneNumber(phone!);
            var EmployeeName = employee!.Name ?? "Unknown";
            var deleted = _employeeService.DeleteEmployee(idToDelete);
            string? refererUrl = Request.Headers["Referer"].ToString();

            return Redirect(refererUrl);

        }
        [Authorize]
        [HttpPost]
        public IActionResult ConvertDelete([FromQuery] Guid id)
        {

            if (id == Guid.Empty)
            {
                return Json(new { success = false, message = "Must Enter The Id Or Phone To Delete ❌" });
            }
            EmployeeVM? employee = null;
            employee = _employeeService.GetEmployeeById(id);

            var EmployeeName = employee!.Name ?? "Unknown";
            var deleted = _employeeService.ConvertDelete(id);
            string? refererUrl = Request.Headers["Referer"].ToString();
            if (deleted == "Employee Not Found")
            {
                return RedirectToAction(refererUrl);
            }
            else
            {
                return RedirectToAction(refererUrl);
            }
        }
        [Authorize]
        [HttpGet]
        public IActionResult GetEmployeeById([FromRoute] Guid id)
        {
            
            var employee = _employeeService.GetEmployeeById(id);
            if (employee == null)
            {
                return Json(new { success = false, message = "Employee Not Found ❌", data = employee });
            }
            return Json(new { success = true, message = "Employee Has Returned ✅", data = employee });
        }
        [Authorize]
        [HttpGet]
        public IActionResult GetEmployeeByPhoneNumber(string phone)
        {
            
            var employee = _employeeService.GetEmployeeByPhoneNumber(phone);
            if (employee == null)
            {
                return Json(new { success = false, message = "Employee Not Found ❌", data = employee });
            }
            return Json(new { success = true, message = $"Employee: {employee.Name} Has Returned ✅", data = employee });
        }
    }
}
