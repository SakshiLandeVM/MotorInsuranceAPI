
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MotorInsuranceAPI.Data;
using MotorInsuranceAPI.DTOs;
using MotorInsuranceAPI.Models;

namespace MotorInsuranceAPI.Services
{
    public class ClaimService : IClaimService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ClaimService(
            ApplicationDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Get paginated and sorted claims for the logged-in user.
        public async Task<(List<ClaimResponseDto> Items, int TotalRecords)>
            GetAllAsync(
                int userId,
                int pageNumber,
                int pageSize,
                string sortBy,
                string sortOrder)
        {
            var query = _context.Claims
                .AsNoTracking()
                .Where(c => c.UserId == userId);

            // Count all matching claims before pagination.
            int totalRecords = await query.CountAsync();

            bool ascending = sortOrder.Equals(
                "asc",
                StringComparison.OrdinalIgnoreCase);

            // Apply sorting before skipping or taking records.
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
                .ToListAsync();

            return (
                _mapper.Map<List<ClaimResponseDto>>(claims),
                totalRecords
            );
        }

        // Get one claim belonging to the logged-in user.
        public async Task<ClaimResponseDto?> GetByIdAsync(
            int id,
            int userId)
        {
            var claim = await _context.Claims
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    c => c.Id == id && c.UserId == userId);

            if (claim == null)
            {
                return null;
            }

            return _mapper.Map<ClaimResponseDto>(claim);
        }

        // Create a claim and save its image path.
        public async Task<ClaimResponseDto> CreateAsync(
            ClaimCreateDto dto,
            int userId,
            string? damageImagePath = null)
        {
            var claim = _mapper.Map<Claim>(dto);

            claim.VehicleNumber = dto.VehicleNumber
                .Trim()
                .ToUpperInvariant();

            claim.Status = "Pending";
            claim.DamageImagePath = damageImagePath;
            claim.CreatedAt = DateTime.UtcNow;
            claim.UserId = userId;

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            return _mapper.Map<ClaimResponseDto>(claim);
        }

        // Update a claim belonging to the logged-in user.
        public async Task<bool> UpdateAsync(
            int id,
            ClaimCreateDto dto,
            int userId)
        {
            var claim = await _context.Claims
                .FirstOrDefaultAsync(
                    c => c.Id == id && c.UserId == userId);

            if (claim == null)
            {
                return false;
            }

            _mapper.Map(dto, claim);

            claim.VehicleNumber = dto.VehicleNumber
                .Trim()
                .ToUpperInvariant();

            await _context.SaveChangesAsync();

            return true;
        }

        // Delete a claim belonging to the logged-in user.
        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var claim = await _context.Claims
                .FirstOrDefaultAsync(
                    c => c.Id == id && c.UserId == userId);

            if (claim == null)
            {
                return false;
            }

            _context.Claims.Remove(claim);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
