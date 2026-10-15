using ElDarBLL.ViewModel.EmailVM;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ElDarBLL.Helpers.EmailService
{
    public class EmailService
    {
        private readonly IOptions<EmailSettings> _options;

        public EmailService(IOptions<EmailSettings> options)
        {
            _options = options;
        }
        public async Task SendEmail(EmailVM email)
        {
            var mail = new MimeMessage()
            {
                Sender = MailboxAddress.Parse(_options.Value.Email),
                Subject = email.Subject,
            };
            mail.To.Add(MailboxAddress.Parse(email.To));

            if(!string.IsNullOrEmpty(email.Bcc))
            {
                mail.Bcc.Add(MailboxAddress.Parse(email.Bcc));
            }

            mail.From.Add(new MailboxAddress(_options.Value.DisplayName, _options.Value.Email));
            var builder = new BodyBuilder();
            builder.HtmlBody = email.Body;
            mail.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(_options.Value.Host, int.Parse(_options.Value.Port), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_options.Value.Email, _options.Value.Password);
            await smtp.SendAsync(mail);
            await smtp.DisconnectAsync(true);
        }
    }
}
