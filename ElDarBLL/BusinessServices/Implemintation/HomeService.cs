using AutoMapper;
using ElDarBLL.BusinessServices.Interfaces;
using ElDarBLL.Helpers;
using ElDarBLL.ViewModel.DataVM;
using ElDarBLL.ViewModel.EmployeeVM;
using ElDarBLL.ViewModel.GuestVM;
using ElDarBLL.ViewModel.HomeVM;
using ElDarDal.Context;
using ElDarDal.Models;
using ElDarDal.Models.Enums;
using ElDarDal.UnitOfWork;

public class HomeService : IHomeService
{
    private readonly ElDarDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public HomeService(ElDarDbContext dbContext , IMapper mapper, IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public List<string> GetMonthDate(DateTime? selectedDate = null)
    {
        var ListOfDates = new List<string>();
        var baseDate = selectedDate ?? DateTime.Now;
        var FirstDayOfMonth = new DateTime(baseDate.Year, baseDate.Month, 1);
        ListOfDates.Add(FirstDayOfMonth.ToString("yyyy-MM-dd"));
        ListOfDates.Add(baseDate.ToString("yyyy-MM-dd"));
        return ListOfDates;
    }
    public IEnumerable<DataVM> DataMonthely(DateTime? selectedDate = null)
    {
        var baseDate = selectedDate ?? DateTime.Now;
        var firstDay = new DateTime(baseDate.Year, baseDate.Month, 1);
        var data = _dbContext.Data
            .Where(x => x.Date >= firstDay && x.Date <= baseDate).OrderByDescending(x => x.CreatedAt)
            .ToList();
        return _mapper.Map<IEnumerable<Data>, IEnumerable<DataVM>>(data);
    }

    public List<string> GetYearDate(DateTime? selectedDate)
    {
        var ListOfDates = new List<string>();
        var baseDate = selectedDate ?? DateTime.Now;
        var FirstDayOfMonth = new DateTime(baseDate.Year, 1, 1);
        ListOfDates.Add(FirstDayOfMonth.ToString("yyyy-MM-dd"));
        ListOfDates.Add(baseDate.ToString("yyyy-MM-dd"));
        return ListOfDates;
    }

    public IEnumerable<DataVM> DataYearly(DateTime? selectedDate)
    {
        var baseDate = selectedDate ?? DateTime.Now;
        var firstDay = new DateTime(baseDate.Year, 1, 1);
        var data = _dbContext.Data
            .Where(x => x.Date >= firstDay && x.Date <= baseDate).OrderByDescending(x => x.CreatedAt)
            .ToList();
        return _mapper.Map<IEnumerable<Data>, IEnumerable<DataVM>>(data);
    }


    public SearchViewModel SearchCriteria(
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
        var result = new SearchViewModel();

        #region 1. Guests Search
        if (searchTarget == "Guests")
        {
            IQueryable<Guest> query = _dbContext.Guests.Where(g => !g.IsDeleted);

            switch (searchBy)
            {
                case "Name":
                    if (searchQuery != "*")
                        query = query.Where(g => g.Name.ToLower().Contains(searchQuery.Trim().ToLower()));
                    break;

                case "PhoneNumber":
                    if (searchQuery != "*")
                        query = query.Where(g => g.PhoneNumber.Contains(searchQuery.Trim()));
                    break;

                case "CardNumber":
                    if (searchQuery != "*")
                        query = query.Where(g => g.CardNumber.Contains(searchQuery.Trim()));
                    break;

                case "Date":
                    if (searchDate.HasValue)
                        query = query.Where(g => g.CreatedAt.Date == searchDate.Value.Date);
                    break;

                case "Status":
                    if (!string.IsNullOrEmpty(searchStatus))
                        query = query.Where(g => g.Status.ToString() == searchStatus);
                    break;
            }

            query = query.OrderByDescending(g => g.CreatedAt);
            var totalCount = query.Count();
            var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            result.Guests = new PaginatedList<Guest>(items, totalCount, pageNumber, pageSize);
        }
        #endregion

        #region 2. Employees Search
        else if (searchTarget == "Employees")
        {
            IQueryable<Employee> query = _dbContext.Employees.Where(e => !e.IsDeleted);

            switch (searchBy)
            {
                case "Name":
                    if (searchQuery != "*")
                        query = query.Where(e => e.Name.ToLower().Contains(searchQuery.Trim().ToLower()));
                    break;

                case "PhoneNumber":
                    if (searchQuery != "*")
                        query = query.Where(e => e.PhoneNumber.Contains(searchQuery.Trim()));
                    break;

                case "CardNumber":
                    if (searchQuery != "*")
                        query = query.Where(e => e.CardNumber.Contains(searchQuery.Trim()));
                    break;

                case "Date":
                    if (searchDate.HasValue)
                        query = query.Where(e => e.CreatedAt.Date == searchDate.Value.Date);
                    break;

                case "Job":
                    if (searchJob.HasValue)
                        query = query.Where(e => e.Job == searchJob.Value);
                    break;

                case "Status":
                    if (!string.IsNullOrEmpty(searchStatus))
                        query = query.Where(e => e.Status.ToString() == searchStatus);
                    break;
            }

            query = query.OrderByDescending(e => e.CreatedAt);
            var totalCount = query.Count();
            var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            result.Employees = new PaginatedList<Employee>(items, totalCount, pageNumber, pageSize);
        }
        #endregion

        #region 3. Data Search
        else if (searchTarget == "Data")
        {
            IQueryable<Data> query = _dbContext.Data.Where(d => !d.IsDeleted);

            switch (searchBy)
            {
                case "Name":
                    if (searchQuery != "*")
                        query = query.Where(d => d.Name.ToLower().Contains(searchQuery.Trim().ToLower()));
                    break;

                case "Date":
                    if (searchDate.HasValue)
                        query = query.Where(d => d.Date.Date == searchDate.Value.Date);
                    break;

                case "Revenues":
                    if (searchRevenue.HasValue)
                        query = query.Where(d => d.Revenues == searchRevenue.Value);
                    break;

                case "Payments":
                    if (searchPayment.HasValue)
                        query = query.Where(d => d.Payments == searchPayment.Value);
                    break;
            }

            query = query.OrderByDescending(d => d.CreatedAt);
            var totalCount = query.Count();
            var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            var mappedItems = _mapper.Map<List<Data>>(items);

            result.DataList = new PaginatedList<Data>(mappedItems, totalCount, pageNumber, pageSize);
        }
        #endregion

        return result;
    }

}