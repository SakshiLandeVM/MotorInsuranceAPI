
using AutoMapper;
using MotorInsuranceAPI.DTOs;
using MotorInsuranceAPI.Models;

namespace MotorInsuranceAPI.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map incoming claim data to the Claim model.
            CreateMap<ClaimCreateDto, Claim>();

            // Map the Claim model to the response DTO.
            CreateMap<Claim, ClaimResponseDto>();
        }
    }
}
