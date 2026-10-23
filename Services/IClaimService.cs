
using MotorInsuranceAPI.DTOs;

namespace MotorInsuranceAPI.Services
{
    // Defines the operations available in the claims service.
    public interface IClaimService
    {
        Task<(List<ClaimResponseDto> Items, int TotalRecords)> GetAllAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string sortBy,
            string sortOrder);

        Task<ClaimResponseDto?> GetByIdAsync(
            int id,
            int userId);

        Task<ClaimResponseDto> CreateAsync(
            ClaimCreateDto dto,
            int userId,
            string? damageImagePath = null);

        Task<bool> UpdateAsync(
            int id,
            ClaimCreateDto dto,
            int userId);

        Task<bool> DeleteAsync(
            int id,
            int userId);
    }
}
