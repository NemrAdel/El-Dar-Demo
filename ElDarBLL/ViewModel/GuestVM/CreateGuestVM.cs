using ElDarDal.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElDarBLL.ViewModel.GuestVM
{
    public class CreateGuestVM
    {
        public string CardNumber { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public decimal Price { get; set; }
        public decimal? OutPrice { get; set; }
        public Status? Status { get; set; } = ElDarDal.Models.Enums.Status.Active;
        public string? Notes { get; set; }
    }
}
