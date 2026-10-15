
using ElDarDal.Models.Enums;

namespace ElDarBLL.ViewModel.DataVM
{
    public class CreateDataVM
    {
        public string Name { get; set; } = null!;
        public Revenues? Revenues { get; set; }
        public Payments? Payments { get; set; }
        public decimal? Price { get; set; }
        public decimal? OutPrice { get; set; }
        public decimal? Payment { get; set; }
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
    }
}
