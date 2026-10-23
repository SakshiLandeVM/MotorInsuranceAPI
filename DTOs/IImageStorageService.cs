
using Microsoft.AspNetCore.Http;

namespace MotorInsuranceAPI.Services
{
    public interface IImageStorageService
    {
        Task<string> SaveClaimImageAsync(IFormFile image);
    }
}
