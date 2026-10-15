
using ElDarBLL.Helpers;
using ElDarBLL.ViewModel.GuestVM;

namespace ElDarBLL.BusinessServices.Interfaces
{
    public interface IGuestService
    {
        IEnumerable<GuestVM> GetAllGuests();
        IEnumerable<GuestVM> GetAllGuestsAll();
        GuestVM? GetGuestById(Guid id);
        Guid GetGuestIdByPhoneNumber(string phone);
        GuestVM GetGuestCard(string CardNumber);
        GuestVM? GetGuestByPhoneNumber(string phone);
        bool AddGuest(CreateGuestVM guest);
        bool UpdateGuest(Guid id, UpdateGuestVM guest);
        bool DeleteGuest(Guid id);
        string ConvertDelete(Guid id);
        PaginatedList<GuestVM> GetPagedData(int pageNumber = 1, int pageSize = 15, bool includeDeleted = false, bool isActive = true);

        //bool MarkGuestAsPaid(Guid id);
    }
}
