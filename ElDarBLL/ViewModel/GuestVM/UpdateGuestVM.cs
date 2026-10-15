using ElDarDal.Models.Enums;

namespace ElDarBLL.ViewModel.GuestVM
{
    public class UpdateGuestVM
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; } = null!;
        public string? CardNumber { get; set; } = null!;
        public string? PhoneNumber { get; set; } = null!;
        public string? Address { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public decimal? Price { get; set; }
        public decimal? OutPrice { get; set; }
        public Status? Status { get; set; }
        public string? Notes { get; set; }
        public string? IsPaid { get; set; }
    }
}
