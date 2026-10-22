using Base.Helpers.Dtos;
using Core.Dtos;

namespace Core.Interfaces.Services
{
    public interface ISigningHttpService
    {
        Task<ResponseDto<ProvisionAgentSigningAuthorityResponseDto>> ProvisionAgentSigningAuthorityAsync(ProvisionAgentSigningAuthorityRequestDto request);
    }
}