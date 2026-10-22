using Base.Helpers.Dtos;
using System.Security.Claims;

namespace Base.Services.JWTToken
{
    public interface ITokenService
    {
        string CreateByOnlyClaims(List<Claim> claims, JWTTokenConfig config);
    }
}
