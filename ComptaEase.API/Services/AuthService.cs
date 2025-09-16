using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ComptaEase.API.DTOs;
using ComptaEase.API.Models;
using ComptaEase.API.Data;
using Microsoft.EntityFrameworkCore;

namespace ComptaEase.API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IConfiguration configuration,
        AppDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || !user.IsActive)
            return null;

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded)
            return null;

        var company = await _context.Companies.FindAsync(user.CompanyId);
        if (company == null)
            return null;

        var token = await GenerateTokenAsync(user.Id, user.CompanyId, user.Role.ToString());

        return new LoginResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString(),
                CompanyId = user.CompanyId
            },
            Company = new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                Address = company.Address,
                Phone = company.Phone,
                Email = company.Email,
                TaxId = company.TaxId
            }
        };
    }

    public async Task<LoginResponseDto?> RegisterAsync(RegisterDto registerDto)
    {
        // Create company first
        var company = new Company
        {
            Name = registerDto.CompanyName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        // Create user
        var user = new User
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            CompanyId = company.Id,
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            // Remove company if user creation failed
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return null;
        }

        var token = await GenerateTokenAsync(user.Id, user.CompanyId, user.Role.ToString());

        return new LoginResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString(),
                CompanyId = user.CompanyId
            },
            Company = new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                Address = company.Address,
                Phone = company.Phone,
                Email = company.Email,
                TaxId = company.TaxId
            }
        };
    }

    public Task<string> GenerateTokenAsync(string userId, int companyId, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("CompanyId", companyId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds);

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
}