
using System.ComponentModel.DataAnnotations;

namespace MotorInsuranceAPI.DTOs
{
    public class UpdateClaimStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
