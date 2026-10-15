using ElDarDal.Models.Enums;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace El_Dar.Models
{

    [Table("Employees")]
    public class SupabaseEmployee : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("IsSynced")]
        public bool IsSynced { get; set; }

        [Column("IsDeleted")]
        public bool IsDeleted { get; set; }

        [Column("CardNumber")]
        public string CardNumber { get; set; } = null!;

        [Column("Name")]
        public string Name { get; set; } = null!;

        [Column("PhoneNumber")]
        public string PhoneNumber { get; set; } = null!;

        [Column("Address")]
        public string Address { get; set; } = null!;

        [Column("DateOfBirth")]
        public DateTime DateOfBirth { get; set; }
        [Column("Salary")]
        public decimal Salary { get; set; }
        [Column("Adds")]
        public decimal? Adds { get; set; }
        [Column("Taxs")]
        public decimal? Taxs { get; set; }
        [Column("Total")]
        public decimal? Total { get; set; }
        [Column("Job")]
        public string Job { get; set; } = null!;
        [Column("Status")]
        public string Status { get; set; } = "Active";

        [Column("IsPaid")]
        public bool IsPaid { get; set; } = true;
        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }
        [Column("CreatedAt")]
        public DateTime? CreatedAt { get; set; }

        [Column("Notes")]
        public string? Notes { get; set; }
    }
}
