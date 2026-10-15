using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace El_Dar.Models
{
    [Table("DeletedLogs")]
    public class SupabaseDeletedLogs : BaseModel
    {
        [PrimaryKey("id", true)]
        public Guid Id { get; set; }

        [Column("TableName")]
        public string TableName { get; set; } = null!;

        [Column("DeletedAt")]
        public DateTime DeletedAt { get; set; }
    }
}
