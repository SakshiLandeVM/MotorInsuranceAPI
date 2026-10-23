
namespace MotorInsuranceAPI.Services
{
    public interface IEmailNotificationService
    {
        Task SendClaimStatusEmailAsync(
            string customerEmail,
            string customerName,
            int claimId,
            string status
        );
    }
}
