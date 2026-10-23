
namespace MotorInsuranceAPI.DTOs
{
    // Defines the claim information returned by the API.
    public class ClaimResponseDto
    {
        public int Id { get; set; }

        public string VehicleNumber { get; set; } = string.Empty;

        public decimal ClaimAmount { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? DamageImagePath { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
