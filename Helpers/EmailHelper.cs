using MimeKit;
using MimeKit.Text;

namespace AnyoneForTennis.Helpers
{
    public class EmailHelper
    {
        private readonly IConfiguration _configuration;

        public EmailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_configuration["SmtpSettings:SenderName"], _configuration["SmtpSettings:SenderEmail"]));
            email.To.Add(new MailboxAddress(toEmail, toEmail));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Plain) { Text = body };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            var server = _configuration["SmtpSettings:Server"] ?? throw new InvalidOperationException("SMTP server not configured.");
            var port = int.Parse(_configuration["SmtpSettings:Port"] ?? throw new InvalidOperationException("SMTP port not configured."));
            var username = _configuration["SmtpSettings:Username"] ?? throw new InvalidOperationException("SMTP username not configured.");
            var password = _configuration["SmtpSettings:Password"] ?? throw new InvalidOperationException("SMTP password not configured.");

            await smtp.ConnectAsync(server, port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(username, password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
