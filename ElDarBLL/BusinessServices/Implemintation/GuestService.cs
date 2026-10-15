using AutoMapper;
using ElDarBLL.BusinessServices.Interfaces;
using ElDarBLL.Helpers;
using ElDarBLL.ViewModel.GuestVM;
using ElDarDal.Context;
using ElDarDal.Models;
using ElDarDal.Models.Enums;
using ElDarDal.UnitOfWork;

namespace ElDarBLL.BusinessServices.Implemintation
{
    public class GuestService : IGuestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ElDarDbContext _dbContext;
        private readonly IMapper _mapper;

        public GuestService
            (
            IUnitOfWork unitOfWork,
            ElDarDbContext dbContext,
            IMapper mapper
            )
        {
            _unitOfWork = unitOfWork;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public bool AddGuest(CreateGuestVM guest)
        {
            try
            {
            if (guest == null)
                return false;
            var PhoneNumberExist = _dbContext.Guests.Any(g => g.PhoneNumber == guest.PhoneNumber);
            if ( PhoneNumberExist ) 
                return false;

            var mappedGuest = _mapper.Map<Guest>(guest);
            var now = DateTime.UtcNow;
            mappedGuest.CreatedAt = now;
            mappedGuest.UpdatedAt = now;
            mappedGuest.IsSynced = false; 

            _unitOfWork.GetRepository<Guest>().Add(mappedGuest);
            var Created= _unitOfWork.SaveChanges();
            return Created>0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting employee: {ex.Message}");
                return false;
            }

        }

        public bool DeleteGuest(Guid id)
        {
            var guest = _unitOfWork.GetRepository<Guest>().GetById(id);
            if (guest == null)
                return false;
            _dbContext.DeletedLogs.Add(new DeletedLogs
            {
                Id = id,
                TableName = "Guests",
            });
            _unitOfWork.GetRepository<Guest>().Delete(guest);
            var Deleted = _unitOfWork.SaveChanges();
            return Deleted > 0;
        }

        public IEnumerable<GuestVM> GetAllGuests()
        {
            var guests = _unitOfWork.GetRepository<Guest>().GetAll(g =>
                !g.IsDeleted &&
                g.Status != Status.Leave &&
                g.Status != Status.Dead);
            if (guests == null || !guests.Any()) return [];
            return _mapper.Map<IEnumerable<GuestVM>>(guests);
        }

        public GuestVM? GetGuestById(Guid id)
        {
            var guest = _unitOfWork.GetRepository<Guest>().GetById(id);
            if (guest == null) return null;
            return _mapper.Map<GuestVM>(guest);
        }

        public Guid GetGuestIdByPhoneNumber(string phone)
        {
            var guest = _dbContext.Guests.FirstOrDefault(g => g.PhoneNumber == phone);
            if (guest == null)
                return Guid.Empty;
            return guest.Id;
        }

        public GuestVM? GetGuestByPhoneNumber(string phone)
        {
            var guest = _dbContext.Guests.FirstOrDefault(g => g.PhoneNumber == phone);
            if (guest == null)
                return null;
            return _mapper.Map<GuestVM>(guest);
        }

        public bool UpdateGuest(Guid id, UpdateGuestVM guest)
        {
            var Existguest= _unitOfWork.GetRepository<Guest>().GetById(id);
            if (Existguest == null)
                return false;
            var mappedGuest = _mapper.Map(guest, Existguest);
            var now = DateTime.UtcNow;
            mappedGuest.UpdatedAt= now;
            mappedGuest.IsSynced = false;
            _unitOfWork.GetRepository<Guest>().Update(mappedGuest);
            var Updated = _unitOfWork.SaveChanges();
            return Updated > 0;

        }

        public IEnumerable<GuestVM> GetAllGuestsAll()
        {
            var guests = _unitOfWork.GetRepository<Guest>().GetAll();
            if (guests == null || !guests.Any()) return [];
            return _mapper.Map<IEnumerable<GuestVM>>(guests);
        }

        public GuestVM GetGuestCard(string CardNumber)
        {
            var guest = _dbContext.Guests.FirstOrDefault(g => g.CardNumber == CardNumber);
            if (guest == null) return null!;
            return _mapper.Map<GuestVM>(guest);
        }
        public string ConvertDelete(Guid id)
        {
            var guest = _unitOfWork.GetRepository<Guest>().GetById(id);
            if (guest == null)
            {
                return "Guest Not Found";
            }
            if (guest.IsDeleted)
                guest.IsDeleted = false;
            else
                guest.IsDeleted = true;

            guest.IsSynced = false;
            guest.UpdatedAt = DateTime.UtcNow;
            var isDeleted = _unitOfWork.SaveChanges();
            return "Converted Successfully";
        }

        public PaginatedList<GuestVM> GetPagedData(int pageNumber = 1, int pageSize = 15, bool includeDeleted = false, bool isActive = true)
        {
            var query = includeDeleted ? _unitOfWork.GetRepository<Guest>().GetAll().OrderByDescending(g => g.CreatedAt)
                : _unitOfWork.GetRepository<Guest>().GetAll(g => !g.IsDeleted && g.Status == Status.Active).OrderByDescending(g => g.CreatedAt);

            var totalCount = query.Count();
            var items = query.Skip((pageNumber - 1) * pageSize)
                             .Take(pageSize)
                             .ToList();
            var mappedItems = _mapper.Map<List<GuestVM>>(items);
            return new PaginatedList<GuestVM>(mappedItems, totalCount, pageNumber, pageSize);
        }



        //public bool MarkGuestAsPaid(Guid id)
        //{
        //    var guest = _unitOfWork.GetRepository<Guest>().GetById(id);
        //    if (guest == null)
        //        return false;
        //    guest.IsPaid = true;

        //}
    }
}
