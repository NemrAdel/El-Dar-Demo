using ElDarDal.Models.Enums;

namespace ElDarBLL.ViewModel.DataVM
{
    public class DataVM
    {
        public Guid? Id { get; set; }
        public string Name { get; set; } = null!;
        public Revenues? Revenues { get; set; }
        public Payments? Payments { get; set; }
        public decimal? Price { get; set; }
        public decimal? OutPrice { get; set; }
        public decimal? Payment { get; set; }
        public DateTime Date { get; set; }
        public string? IsSynced { get; set; }
        public string? IsDeleted { get; set; }
        public string? Notes { get; set; }
    }
}
