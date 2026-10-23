
using System.ComponentModel.DataAnnotations;

namespace MotorInsuranceAPI.DTOs
{
    // Defines the data accepted when creating or updating a claim.
    public class ClaimCreateDto
    {
        // Vehicle registration number is required.
        [Required(ErrorMessage = "Vehicle number is required.")]
        [StringLength(
            20,
            ErrorMessage = "Vehicle number cannot exceed 20 characters."
        )]
        public string VehicleNumber { get; set; } = string.Empty;

        // Claim amount must be greater than zero.
        [Range(
            typeof(decimal),
            "0.01",
            "9999999999999999.99",
            ErrorMessage = "Claim amount must be greater than zero."
        )]
        public decimal ClaimAmount { get; set; }
    }
}
