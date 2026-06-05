using System.Security.Claims;
using JobProcessing.Api.Models;

namespace JobProcessing.Api.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateAccessToken(string token);
    DateTime AccessTokenExpiresAt();
}