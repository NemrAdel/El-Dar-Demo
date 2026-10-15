
namespace ElDarDal.Models
{
    public class DeletedLogs
    {
        public Guid Id { get; set; }

        public string TableName { get; set; } = null!;
        public DateTime DeletedAt { get; set; } 
    }
}
