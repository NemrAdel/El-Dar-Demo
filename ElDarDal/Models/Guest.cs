using ElDarDal.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElDarDal.Models
{
    public class Guest:BaseEntity
    {

        public string CardNumber { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string Address { get; set; } = null!;

        public DateTime DateOfBirth { get; set; }

        [NotMapped]
        public int Age => DateTime.Now.Year - DateOfBirth.Year;

        public decimal Price { get; set; }
        public decimal? OutPrice { get; set; }
        public Status Status { get; set; }= Status.Active;

        public bool IsPaid { get; set; } = true;

        public string? Notes { get; set; }

    }
}
