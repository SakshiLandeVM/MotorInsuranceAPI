
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MotorInsuranceAPI.DTOs
{
    public class ClaimCreateWithImageDto
    {
        [Required]
        [StringLength(20)]
        public string VehicleNumber { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "9999999999999999.99")]
        public decimal ClaimAmount { get; set; }

        [Required]
        public IFormFile DamageImage { get; set; } = null!;
    }
}
