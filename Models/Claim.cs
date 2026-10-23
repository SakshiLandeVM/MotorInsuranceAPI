
using System.ComponentModel.DataAnnotations;

namespace MotorInsuranceAPI.Models
{
    public class Claim
    {
        // Unique ID for each claim.
        public int Id { get; set; }

        // Registration number of the vehicle.
        [Required]
        [StringLength(10)]
        public string VehicleNumber { get; set; } = string.Empty;

        // Amount requested for the insurance claim.
        [Range(typeof(decimal), "0.01", "999999999.99")]
        public decimal ClaimAmount { get; set; }

        // Default status of a newly created claim.
        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending";

        // Optional path to the vehicle damage image.
        [StringLength(500)]
        public string? DamageImagePath { get; set; }

        // Date and time when the claim was created.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ID of the user who owns this claim.
        public int UserId { get; set; }

        // Navigation property linking the claim to its user.
        public User? User { get; set; }
    }
}
