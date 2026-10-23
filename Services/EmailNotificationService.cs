
using System.Net;
using System.Net.Mail;

namespace MotorInsuranceAPI.Services
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(
            IConfiguration configuration,
            ILogger<EmailNotificationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendClaimStatusEmailAsync(
            string customerEmail,
            string customerName,
            int claimId,
            string status)
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPortText = _configuration["Email:SmtpPort"];
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            var fromAddress = _configuration["Email:FromAddress"];
            var fromName = _configuration["Email:FromName"];

            if (string.IsNullOrWhiteSpace(smtpHost) ||
                !int.TryParse(smtpPortText, out var smtpPort) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fromAddress))
            {
                throw new InvalidOperationException(
                    "Email SMTP settings are missing. Configure the Email settings before sending email.");
            }

            using var message = new MailMessage();
            message.From = new MailAddress(
                fromAddress,
                string.IsNullOrWhiteSpace(fromName)
                    ? "Motor Insurance"
                    : fromName);

            message.To.Add(customerEmail);
            message.Subject = $"Motor Insurance Claim #{claimId} - {status}";
            message.Body = $"""
                Hello {customerName},

                Your motor insurance claim #{claimId} has been {status.ToLowerInvariant()}.

                Status: {status}

                If you have questions, please contact our support team.

                Regards,
                Motor Insurance Team
                """;
            message.IsBodyHtml = false;

            using var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(username, password)
            };

            try
            {
                await smtpClient.SendMailAsync(message);
                _logger.LogInformation(
                    "Claim status email sent for claim {ClaimId}.",
                    claimId);
            }
            catch (SmtpException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send claim status email for claim {ClaimId}.",
                    claimId);
                throw;
            }
        }
    }
}
