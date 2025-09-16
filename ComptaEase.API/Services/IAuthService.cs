using ComptaEase.API.DTOs;

namespace ComptaEase.API.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
    Task<LoginResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<string> GenerateTokenAsync(string userId, int companyId, string role);
}