
namespace ElDarBLL.ViewModel.EmployeeVM
{
    public class UpdateEmployeeVM
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; } = null!;
        public string? CardNumber { get; set; } = null!;
        public string? PhoneNumber { get; set; } = null!;
        public string? Address { get; set; } = null!;
        public string? Job { get; set; } = null!;
        public string? Status { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public decimal? Salary { get; set; }
        public decimal? Adds { get; set; }
        public decimal? Taxs { get; set; }
        public string? Notes { get; set; }
        public bool? Ispaid { get; set; }
    }
}
