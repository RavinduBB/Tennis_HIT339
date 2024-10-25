using AnyoneForTennis.Helpers;

namespace AnyoneForTennis.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailHelper _emailHelper;

        public EmailService(EmailHelper emailHelper)
        {
            _emailHelper = emailHelper;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            await _emailHelper.SendEmailAsync(toEmail, subject, body);
        }
    }
}
