
namespace ElDarBLL.ViewModel.EmailVM
{
    public class EmailVM
    {
        public string To { get; set; } = null!;
        public string? Bcc { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
    }
}
