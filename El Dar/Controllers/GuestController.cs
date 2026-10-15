using ElDarBLL.BusinessServices.Interfaces;
using ElDarBLL.ViewModel.GuestVM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace El_Dar.Controllers
{
    public class GuestController : Controller
    {
        private readonly IGuestService _guestService;

        public GuestController(IGuestService guestService)
        {
            _guestService = guestService;
        }
        [Authorize]
        [HttpGet]
        public IActionResult Index(int pageNumber = 1, int pageSize = 30 , bool includeDeleted = false, bool isActive = true)
        {
            var guests = _guestService.GetPagedData(pageNumber, pageSize, includeDeleted, isActive);
            return View(guests);
        }
        [Authorize]
        [HttpGet]
        public IActionResult GetAllGuests(int pageNumber = 1, int pageSize = 50 , bool includeDeleted = true)
        {
            var guests = _guestService.GetPagedData(pageNumber, pageSize, includeDeleted);
            return View(guests);
        }
        [Authorize]
        [HttpGet]
        public IActionResult GetGuestById([FromRoute] Guid id)
        {
            var guests = _guestService.GetGuestById(id);
            if (guests == null)
            {
                return Json(new { success = false, message = "No Guests Found ❌" });
            }
            return Json(new { success = true, message = "All Guests Has Rerturned ✅", data = guests });
        }
        [Authorize]
        [HttpGet]
        public IActionResult GetGuestByPhoneNumber(string phone)
        {
            var guest = _guestService.GetGuestByPhoneNumber(phone);
            if (guest == null)
            {
                return Json(new { success = false, message = $"Guest With {phone} Is Not Found ❌" });
            }
            return Json(new { success = true, message = $"Guest: {guest.Name} Has Rerturned ✅", data = guest });
        }
        [Authorize]
        [HttpPost]
        public IActionResult DeleteGuest([FromQuery] Guid? id)
        {
            if (id == null)
            {
                return Json(new { success = false, message = "Must Enter The Id Or Phone To Delete ❌" });
            }

            GuestVM? guest = null;

            if (id.HasValue)
                guest = _guestService.GetGuestById(id.Value);

            var GuestName = guest?.Name ?? "Unknown";
            var deleted = _guestService.DeleteGuest(guest!.Id);
            string? refererUrl = Request.Headers["Referer"].ToString();
            if (!deleted)
            {
                return Redirect(refererUrl);
            }
            else
            {
                return Redirect(refererUrl);
            }

        }
        [Authorize]
        [HttpPost]
        public IActionResult ConvertDelete([FromQuery] Guid id)
        {

            if (id == Guid.Empty)
            {
                return View("GetAllGuests");
            }
            GuestVM? guest = null;
            guest = _guestService.GetGuestById(id);

            var GuestName = guest!.Name ?? "Unknown";
            var deleted = _guestService.ConvertDelete(id);
            string? refererUrl = Request.Headers["Referer"].ToString();

            return Redirect(refererUrl);

        }
        [Authorize]
        [HttpPost]
        public IActionResult CreateGuest([FromForm] CreateGuestVM guest)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var ExistGuest = _guestService.GetGuestByPhoneNumber(guest.PhoneNumber);
            var GuestCard = _guestService.GetGuestCard(guest.CardNumber);

            if (GuestCard != null || ExistGuest != null)
            {
                ViewBag.PhoneAndCardErrorMessage = $"Guest With Phone Number {guest.PhoneNumber} Or Card Number {guest.CardNumber} Already Exists With {GuestCard?.Name ?? ExistGuest.Name} ❌";
                return RedirectToAction("Index" , _guestService.GetPagedData());
            }
            var isCreated = _guestService.AddGuest(guest);
            var createdGuest = _guestService.GetGuestByPhoneNumber(guest.PhoneNumber);
            if (!isCreated)
            {
                ViewBag.FailedToAddGuestMessage = $"Failed To Add Guest With Phone Number {guest.PhoneNumber} ❌";
                return RedirectToAction("Index", _guestService.GetPagedData());
            }
            else
            {
                return RedirectToAction("Index", _guestService.GetPagedData());
            }
        }
        [Authorize]
        [HttpGet]
        public IActionResult UpdateGuestView([FromQuery] Guid? id,string returnUrl = null)
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

            var Guest = _guestService.GetGuestById(id!.Value);
            return View("UpdateGuestView", Guest);
        }
        [Authorize]
        [HttpPost]
        public IActionResult UpdateGuest([FromForm] UpdateGuestVM updateGuest, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("UpdateGuestView", new { id = updateGuest.Id, returnUrl = returnUrl });
            }

            var ExistGuest = _guestService.GetGuestByPhoneNumber(updateGuest.PhoneNumber ?? "");

            if (ExistGuest != null && ExistGuest.Id != updateGuest.Id)
            {
                return RedirectToAction("UpdateGuestView", new { id = updateGuest.Id, returnUrl = returnUrl });
            }

            var GuestCard = _guestService.GetGuestCard(updateGuest.CardNumber ?? "");
            if (GuestCard != null && GuestCard.Id != updateGuest.Id)
            {
                Console.WriteLine($"1 Card is : {GuestCard.Id} 2 Card is : {updateGuest.Id}");
                return RedirectToAction("UpdateGuestView", new { id = updateGuest.Id, returnUrl = returnUrl });
            }

            var isUpdated = _guestService.UpdateGuest(updateGuest.Id.Value, updateGuest);

            if (!isUpdated)
            {
                return RedirectToAction("UpdateGuestView", new { id = updateGuest.Id, returnUrl = returnUrl });
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
