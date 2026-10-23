
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotorInsuranceAPI.DTOs;
using MotorInsuranceAPI.Services;

namespace MotorInsuranceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClaimsController : ControllerBase
    {
        private readonly IClaimService _claimService;
        private readonly IImageStorageService _imageStorageService;

        public ClaimsController(
            IClaimService claimService,
            IImageStorageService imageStorageService)
        {
            _claimService = claimService;
            _imageStorageService = imageStorageService;
        }

        private int CurrentUserId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        
        // GET: api/claims
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] string sortBy = "createdAt",
            [FromQuery] string sortOrder = "desc")
        {
            if (pageNumber < 1)
            {
                return BadRequest(new
                {
                    message = "Page number must be at least 1."
                });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new
                {
                    message = "Page size must be between 1 and 100."
                });
            }

            string[] allowedSortFields =
            {
                "id",
                "createdAt",
                "claimAmount",
                "vehicleNumber",
                "status"
            };

            if (!allowedSortFields.Contains(
                    sortBy,
                    StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Invalid sortBy. Allowed values: id, createdAt, claimAmount, vehicleNumber, status."
                });
            }

            if (!sortOrder.Equals(
                    "asc",
                    StringComparison.OrdinalIgnoreCase)
                && !sortOrder.Equals(
                    "desc",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "sortOrder must be asc or desc."
                });
            }

            var result = await _claimService.GetAllAsync(
                CurrentUserId,
                pageNumber,
                pageSize,
                sortBy,
                sortOrder);

            return Ok(new
            {
                pageNumber,
                pageSize,
                totalRecords = result.TotalRecords,
                totalPages = (int)Math.Ceiling(
                    result.TotalRecords / (double)pageSize),
                data = result.Items
            });
        }


        // GET: api/claims/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var claim = await _claimService.GetByIdAsync(
                id,
                CurrentUserId
            );

            if (claim == null)
            {
                return NotFound(new { message = "Claim not found." });
            }

            return Ok(claim);
        }

        // POST: api/claims
        // Create a claim with a damage image.
        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> Create(
            [FromForm] ClaimCreateWithImageDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            string imagePath;

            try
            {
                imagePath = await _imageStorageService.SaveClaimImageAsync(
                    dto.DamageImage
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            try
            {
                var claimDto = new ClaimCreateDto
                {
                    VehicleNumber = dto.VehicleNumber,
                    ClaimAmount = dto.ClaimAmount
                };

                // Save the claim using the existing service.
                var claim = await _claimService.CreateAsync(
                    claimDto,
                    CurrentUserId,
                    imagePath
                );

                // The image path must also be stored on the claim.
                // This will be connected to the claim service in the next step.
                return CreatedAtAction(
                    nameof(GetById),
                    new { id = claim.Id },
                    new
                    {
                        claim.Id,
                        claim.VehicleNumber,
                        claim.ClaimAmount,
                        claim.Status,
                        claim.CreatedAt,
                        DamageImagePath = imagePath
                    }
                );
            }
            catch
            {
                // Avoid leaving an orphaned file if claim creation fails.
                var physicalPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)
                );

                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }

                throw;
            }
        }

        // PUT: api/claims/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            ClaimCreateDto dto)
        {
            bool updated = await _claimService.UpdateAsync(
                id,
                dto,
                CurrentUserId
            );

            if (!updated)
            {
                return NotFound(new { message = "Claim not found." });
            }

            return Ok(new { message = "Claim updated successfully." });
        }

        // DELETE: api/claims/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _claimService.DeleteAsync(
                id,
                CurrentUserId
            );

            if (!deleted)
            {
                return NotFound(new { message = "Claim not found." });
            }

            return Ok(new { message = "Claim deleted successfully." });
        }
    }
}
