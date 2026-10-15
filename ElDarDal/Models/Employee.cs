using ElDarDal.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElDarDal.Models
{
    public class Employee : BaseEntity
    {
        public string CardNumber { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Address { get; set; } = null!;
        public JobEnum Job { get; set; }
        public Status Status { get; set; }
        public DateTime DateOfBirth { get; set; }

        [NotMapped]
        public int Age => DateTime.Now.Year - DateOfBirth.Year;
        public decimal Salary { get; set; }
        public decimal? Adds { get; set; }
        public decimal? Taxs { get; set; }
        public decimal Total { get; }
        public bool IsPaid { get; set; } = false;
        public string? Notes { get; set; }

    }
}
