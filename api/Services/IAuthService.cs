using JobProcessing.Api.Models.DTOs;

namespace JobProcessing.Api.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, string ipAddress);
    Task<LoginResponse?> RefreshAsync(string refreshToken, string ipAddress);
    Task<bool> RevokeAsync(string refreshToken, string ipAddress);
    Task SeedAdminUserAsync();
}