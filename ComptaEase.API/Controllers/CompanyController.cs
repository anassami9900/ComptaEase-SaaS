using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ComptaEase.API.Data;
using ComptaEase.API.DTOs;
using ComptaEase.API.Models;

namespace ComptaEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompanyController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(AppDbContext context, ILogger<CompanyController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private int GetCompanyId()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        return int.TryParse(companyIdClaim, out var companyId) ? companyId : 0;
    }

    [HttpGet]
    public async Task<ActionResult<CompanyDto>> GetCompany()
    {
        var companyId = GetCompanyId();
        if (companyId == 0) return Unauthorized();

        var company = await _context.Companies
            .Where(c => c.Id == companyId)
            .Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                Address = c.Address,
                Phone = c.Phone,
                Email = c.Email,
                TaxId = c.TaxId
            })
            .FirstOrDefaultAsync();

        if (company == null)
            return NotFound();

        return Ok(company);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCompany([FromBody] UpdateCompanyDto updateCompanyDto)
    {
        var companyId = GetCompanyId();
        if (companyId == 0) return Unauthorized();

        var company = await _context.Companies.FindAsync(companyId);
        if (company == null)
            return NotFound();

        if (!string.IsNullOrEmpty(updateCompanyDto.Name))
            company.Name = updateCompanyDto.Name;
        if (updateCompanyDto.Address != null)
            company.Address = updateCompanyDto.Address;
        if (updateCompanyDto.Phone != null)
            company.Phone = updateCompanyDto.Phone;
        if (updateCompanyDto.Email != null)
            company.Email = updateCompanyDto.Email;
        if (updateCompanyDto.TaxId != null)
            company.TaxId = updateCompanyDto.TaxId;

        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("cotisations")]
    public async Task<ActionResult<IEnumerable<CotisationDto>>> GetCotisations()
    {
        var companyId = GetCompanyId();
        if (companyId == 0) return Unauthorized();

        var cotisations = await _context.Cotisations
            .Where(c => c.CompanyId == companyId)
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .Select(c => new CotisationDto
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type.ToString(),
                EmployeeRate = c.EmployeeRate,
                EmployerRate = c.EmployerRate,
                MaxAmount = c.MaxAmount,
                MinAmount = c.MinAmount,
                IsActive = c.IsActive
            })
            .ToListAsync();

        return Ok(cotisations);
    }

    [HttpPost("cotisations")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<CotisationDto>> CreateCotisation([FromBody] CreateCotisationDto createCotisationDto)
    {
        var companyId = GetCompanyId();
        if (companyId == 0) return Unauthorized();

        if (!Enum.TryParse<CotisationType>(createCotisationDto.Type, out var cotisationType))
        {
            return BadRequest(new { message = "Invalid cotisation type" });
        }

        var cotisation = new Cotisation
        {
            CompanyId = companyId,
            Name = createCotisationDto.Name,
            Type = cotisationType,
            EmployeeRate = createCotisationDto.EmployeeRate,
            EmployerRate = createCotisationDto.EmployerRate,
            MaxAmount = createCotisationDto.MaxAmount,
            MinAmount = createCotisationDto.MinAmount,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Cotisations.Add(cotisation);
        await _context.SaveChangesAsync();

        var cotisationDto = new CotisationDto
        {
            Id = cotisation.Id,
            Name = cotisation.Name,
            Type = cotisation.Type.ToString(),
            EmployeeRate = cotisation.EmployeeRate,
            EmployerRate = cotisation.EmployerRate,
            MaxAmount = cotisation.MaxAmount,
            MinAmount = cotisation.MinAmount,
            IsActive = cotisation.IsActive
        };

        return CreatedAtAction(nameof(GetCotisations), cotisationDto);
    }

    [HttpPut("cotisations/{id}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> UpdateCotisation(int id, [FromBody] UpdateCotisationDto updateCotisationDto)
    {
        var companyId = GetCompanyId();
        if (companyId == 0) return Unauthorized();

        var cotisation = await _context.Cotisations
            .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId);

        if (cotisation == null)
            return NotFound();

        if (!string.IsNullOrEmpty(updateCotisationDto.Name))
            cotisation.Name = updateCotisationDto.Name;
        if (updateCotisationDto.EmployeeRate.HasValue)
            cotisation.EmployeeRate = updateCotisationDto.EmployeeRate.Value;
        if (updateCotisationDto.EmployerRate.HasValue)
            cotisation.EmployerRate = updateCotisationDto.EmployerRate.Value;
        if (updateCotisationDto.MaxAmount.HasValue)
            cotisation.MaxAmount = updateCotisationDto.MaxAmount.Value;
        if (updateCotisationDto.MinAmount.HasValue)
            cotisation.MinAmount = updateCotisationDto.MinAmount.Value;
        if (updateCotisationDto.IsActive.HasValue)
            cotisation.IsActive = updateCotisationDto.IsActive.Value;

        cotisation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class UpdateCompanyDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? TaxId { get; set; }
}

public class CotisationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal EmployeeRate { get; set; }
    public decimal EmployerRate { get; set; }
    public decimal? MaxAmount { get; set; }
    public decimal? MinAmount { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCotisationDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal EmployeeRate { get; set; }
    public decimal EmployerRate { get; set; }
    public decimal? MaxAmount { get; set; }
    public decimal? MinAmount { get; set; }
}

public class UpdateCotisationDto
{
    public string? Name { get; set; }
    public decimal? EmployeeRate { get; set; }
    public decimal? EmployerRate { get; set; }
    public decimal? MaxAmount { get; set; }
    public decimal? MinAmount { get; set; }
    public bool? IsActive { get; set; }
}