using ElDarDal.Models.Enums;

namespace ElDarBLL.ViewModel.GuestVM
{
    public class GuestVM
    {
        public Guid Id { get; set; }
        public string CardNumber { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public int Age { get; set; }
        public decimal Price { get; set; }
        public decimal? OutPrice { get; set; }
        public string Status { get; set; } = null!;
        public string? Notes { get; set; }
        public string? IsPaid { get; set; }
        public string? IsDeleted { get; set; }
        public string? IsSynced{ get; set; }
    }
}
