using Supabase.Postgrest.Models;

namespace ElDarDal.Models
{
    public class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;
        public bool IsSynced { get; set; } = false;
    }
}
