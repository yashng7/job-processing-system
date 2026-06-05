using JobProcessing.Api.Data;
using JobProcessing.Api.Models;
using JobProcessing.Api.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace JobProcessing.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    private readonly int _refreshTokenExpiryDays;

    public AuthService(
        AppDbContext context,
        ITokenService tokenService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
        _refreshTokenExpiryDays = int.Parse(
            _configuration["Jwt:RefreshTokenExpiryDays"] ?? "7");
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, string ipAddress)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u =>
                u.Username == request.Username && u.IsActive);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for username: {Username} from {IP}",
                request.Username, ipAddress);
            return null;
        }

        await RemoveExpiredRefreshTokensAsync(user);

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = CreateRefreshToken(ipAddress);

        user.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {Username} logged in from {IP}", user.Username, ipAddress);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            AccessTokenExpiresAt = _tokenService.AccessTokenExpiresAt(),
            Username = user.Username,
            Role = user.Role
        };
    }

    public async Task<LoginResponse?> RefreshAsync(string refreshToken, string ipAddress)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u =>
                u.RefreshTokens.Any(t => t.Token == refreshToken));

        if (user is null)
        {
            _logger.LogWarning("Refresh token not found from {IP}", ipAddress);
            return null;
        }

        var existingToken = user.RefreshTokens.Single(t => t.Token == refreshToken);

        if (!existingToken.IsActive)
        {
            // Detect token reuse — revoke entire family
            if (existingToken.IsRevoked)
            {
                _logger.LogWarning(
                    "Refresh token reuse detected for user {Username} from {IP}. Revoking all tokens.",
                    user.Username, ipAddress);

                await RevokeAllUserTokensAsync(user, ipAddress, "Token reuse detected");
                await _context.SaveChangesAsync();
            }

            return null;
        }

        var newRefreshToken = CreateRefreshToken(ipAddress);

        existingToken.IsRevoked = true;
        existingToken.RevokedByIp = ipAddress;
        existingToken.ReplacedByToken = newRefreshToken.Token;

        await RemoveExpiredRefreshTokensAsync(user);

        user.RefreshTokens.Add(newRefreshToken);

        var newAccessToken = _tokenService.GenerateAccessToken(user);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Token refreshed for user {Username} from {IP}", user.Username, ipAddress);

        return new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            AccessTokenExpiresAt = _tokenService.AccessTokenExpiresAt(),
            Username = user.Username,
            Role = user.Role
        };
    }

    public async Task<bool> RevokeAsync(string refreshToken, string ipAddress)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u =>
                u.RefreshTokens.Any(t => t.Token == refreshToken));

        if (user is null) return false;

        var token = user.RefreshTokens.SingleOrDefault(t => t.Token == refreshToken);
        if (token is null || !token.IsActive) return false;

        token.IsRevoked = true;
        token.RevokedByIp = ipAddress;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Refresh token revoked for user {Username} from {IP}",
            user.Username, ipAddress);

        return true;
    }

    public async Task SeedAdminUserAsync()
    {
        var adminUsername = _configuration["AdminUser:Username"] ?? "admin";

        if (await _context.Users.AnyAsync(u => u.Username == adminUsername))
            return;

        var adminPassword = _configuration["AdminUser:Password"]
            ?? throw new InvalidOperationException("AdminUser:Password not configured.");

        var admin = new User
        {
            Username = adminUsername,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword, workFactor: 12),
            Role = "Admin",
            IsActive = true
        };

        _context.Users.Add(admin);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin user '{Username}' seeded successfully.", adminUsername);
    }

    private RefreshToken CreateRefreshToken(string ipAddress) => new()
    {
        Token = _tokenService.GenerateRefreshToken(),
        ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays),
        CreatedByIp = ipAddress
    };

    private async Task RemoveExpiredRefreshTokensAsync(User user)
    {
        var expired = user.RefreshTokens
            .Where(t => !t.IsActive)
            .ToList();

        _context.RefreshTokens.RemoveRange(expired);
        await Task.CompletedTask;
    }

    private void RevokeAllUserTokens(User user, string ipAddress, string reason)
    {
        foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
        {
            token.IsRevoked = true;
            token.RevokedByIp = ipAddress;
        }
    }

    private async Task RevokeAllUserTokensAsync(User user, string ipAddress, string reason)
    {
        RevokeAllUserTokens(user, ipAddress, reason);
        await Task.CompletedTask;
    }
}