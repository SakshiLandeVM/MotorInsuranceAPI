
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorInsuranceAPI.Data;
using MotorInsuranceAPI.Models;
using MotorInsuranceAPI.Services;
using MotorInsuranceAPI.DTOs;

namespace MotorInsuranceAPI.Controllers
{
    [ApiController]
    [Route("api/admin/claims")]
    [Authorize(Roles = "Admin")]
    public class AdminClaimsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailNotificationService _emailService;
        private readonly ILogger<AdminClaimsController> _logger;

        public AdminClaimsController(
            ApplicationDbContext context,
            IEmailNotificationService emailService,
            ILogger<AdminClaimsController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: api/admin/claims
        [HttpGet]
        public async Task<IActionResult> GetAllClaims(
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

            var query = _context.Claims.AsNoTracking();

            int totalRecords = await query.CountAsync();

            bool ascending = sortOrder.Equals(
                "asc",
                StringComparison.OrdinalIgnoreCase);

            query = sortBy.ToLowerInvariant() switch
            {
                "id" => ascending
                    ? query.OrderBy(c => c.Id)
                    : query.OrderByDescending(c => c.Id),

                "claimamount" => ascending
                    ? query.OrderBy(c => c.ClaimAmount)
                    : query.OrderByDescending(c => c.ClaimAmount),

                "vehiclenumber" => ascending
                    ? query.OrderBy(c => c.VehicleNumber)
                    : query.OrderByDescending(c => c.VehicleNumber),

                "status" => ascending
                    ? query.OrderBy(c => c.Status)
                    : query.OrderByDescending(c => c.Status),

                _ => ascending
                    ? query.OrderBy(c => c.CreatedAt).ThenBy(c => c.Id)
                    : query.OrderByDescending(c => c.CreatedAt)
                           .ThenByDescending(c => c.Id)
            };

            var claims = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.Id,
                    c.VehicleNumber,
                    c.ClaimAmount,
                    c.Status,
                    c.DamageImagePath,
                    c.CreatedAt,
                    CustomerName = c.User!.FullName,
                    CustomerEmail = c.User!.Email
                })
                .ToListAsync();

            return Ok(new
            {
                pageNumber,
                pageSize,
                totalRecords,
                totalPages = (int)Math.Ceiling(
                    totalRecords / (double)pageSize),
                data = claims
            });
        }

        // PUT: api/admin/claims/{id}/approve
        [HttpPut("{id:int}/approve")]
        public Task<IActionResult> ApproveClaim(int id)
        {
            return UpdateClaimStatus(id, "Approved");
        }

        // PUT: api/admin/claims/{id}/reject
        [HttpPut("{id:int}/reject")]
        public Task<IActionResult> RejectClaim(int id)
        {
            return UpdateClaimStatus(id, "Rejected");
        }

        private async Task<IActionResult> UpdateClaimStatus(
            int id,
            string newStatus)
        {
            var claim = await _context.Claims
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (claim == null)
            {
                return NotFound(new
                {
                    message = $"Claim with ID {id} was not found."
                });
            }

            if (claim.Status == "Approved" ||
                claim.Status == "Rejected")
            {
                return Conflict(new
                {
                    message = "This claim has already been finalized.",
                    claimId = claim.Id,
                    status = claim.Status
                });
            }

            claim.Status = newStatus;

            await _context.SaveChangesAsync();

            bool emailSent = false;

            if (claim.User != null &&
                !string.IsNullOrWhiteSpace(claim.User.Email))
            {
                try
                {
                    await _emailService.SendClaimStatusEmailAsync(
                        claim.User.Email,
                        claim.User.FullName,
                        claim.Id,
                        newStatus);

                    emailSent = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Claim {ClaimId} status changed to {Status}, but the notification email failed.",
                        claim.Id,
                        newStatus);
                }
            }

            return Ok(new
            {
                message = $"Claim {newStatus.ToLowerInvariant()} successfully.",
                claimId = claim.Id,
                status = claim.Status,
                emailSent
            });
        }
    }
}
