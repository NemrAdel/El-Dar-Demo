using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace El_Dar.Models
{
    [Table("Data")]
    public class SupabaseData : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }
        [Column("Name")]
        public string Name { get; set; } = null!;
        [Column("Revenues")]
        public string? Revenues { get; set; }
        [Column("Payments")]
        public string? Payments { get; set; }
        [Column("Price")]
        public decimal? Price { get; set; }
        [Column("OutPrice")]
        public decimal? OutPrice { get; set; }
        [Column("Payment")]
        public decimal? Payment { get; set; }
        [Column("Date")]
        public DateTime Date { get; set; }
        [Column("IsSynced")]
        public bool IsSynced { get; set; }
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; }
        [Column("UpdatedAt")]
        public DateTime? UpdatedAt { get; set; }
        [Column("CreatedAt")]
        public DateTime? CreatedAt { get; set; }
        [Column("Notes")]
        public string? Notes { get; set; }
    }
}
