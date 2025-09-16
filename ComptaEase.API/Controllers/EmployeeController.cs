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
public class EmployeeController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(AppDbContext context, ILogger<EmployeeController> logger)
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
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees()
    {
        var companyId = GetCompanyId();
        if (companyId == 0) return Unauthorized();

        var employees = await _context.Employees
            .Where(e => e.CompanyId == companyId)
            .OrderBy(e => e.FirstName)
            .ThenBy(e => e.LastName)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                EmployeeNumber = e.EmployeeNumber,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Phone = e.Phone,
                Address = e.Address,
                DateOfBirth = e.DateOfBirth,
                HireDate = e.HireDate,
                TerminationDate = e.TerminationDate,
                Position = e.Position,
                Department = e.Department,
                BaseSalary = e.BaseSalary,
                CnssNumber = e.CnssNumber,
                CinNumber = e.CinNumber,
                IsActive = e.IsActive
            })
            .ToListAsync();

        return Ok(employees);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
    {
        var companyId = GetCompanyId();
        if (companyId == 0) return Unauthorized();

        var employee = await _context.Employees
            .Where(e => e.Id == id && e.CompanyId == companyId)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                EmployeeNumber = e.EmployeeNumber,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Phone = e.Phone,
                Address = e.Address,
                DateOfBirth = e.DateOfBirth,
                HireDate = e.HireDate,
                TerminationDate = e.TerminationDate,
                Position = e.Position,
                Department = e.Department,
                BaseSalary = e.BaseSalary,
                CnssNumber = e.CnssNumber,
                CinNumber = e.CinNumber,
                IsActive = e.IsActive
            })
            .FirstOrDefaultAsync();

        if (employee == null)
            return NotFound();

        return Ok(employee);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee([FromBody] CreateEmployeeDto createEmployeeDto)
    {
        var companyId = GetCompanyId();
        if (companyId == 0) return Unauthorized();

        // Check if employee number is unique within company
        var existingEmployee = await _context.Employees
            .AnyAsync(e => e.CompanyId == companyId && e.EmployeeNumber == createEmployeeDto.EmployeeNumber);

        if (existingEmployee)
        {
            return BadRequest(new { message = "Employee number already exists" });
        }

        var employee = new Employee
        {
            CompanyId = companyId,
            EmployeeNumber = createEmployeeDto.EmployeeNumber,
            FirstName = createEmployeeDto.FirstName,
            LastName = createEmployeeDto.LastName,
            Email = createEmployeeDto.Email,
            Phone = createEmployeeDto.Phone,
            Address = createEmployeeDto.Address,
            DateOfBirth = createEmployeeDto.DateOfBirth,
            HireDate = createEmployeeDto.HireDate,
            Position = createEmployeeDto.Position,
            Department = createEmployeeDto.Department,
            BaseSalary = createEmployeeDto.BaseSalary,
            CnssNumber = createEmployeeDto.CnssNumber,
            CinNumber = createEmployeeDto.CinNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        var employeeDto = new EmployeeDto
        {
            Id = employee.Id,
            EmployeeNumber = employee.EmployeeNumber,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            Address = employee.Address,
            DateOfBirth = employee.DateOfBirth,
            HireDate = employee.HireDate,
            TerminationDate = employee.TerminationDate,
            Position = employee.Position,
            Department = employee.Department,
            BaseSalary = employee.BaseSalary,
            CnssNumber = employee.CnssNumber,
            CinNumber = employee.CinNumber,
            IsActive = employee.IsActive
        };

        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employeeDto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto updateEmployeeDto)
    {
        var companyId = GetCompanyId();
        if (companyId == 0) return Unauthorized();

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == id && e.CompanyId == companyId);

        if (employee == null)
            return NotFound();

        // Update only provided fields
        if (!string.IsNullOrEmpty(updateEmployeeDto.FirstName))
            employee.FirstName = updateEmployeeDto.FirstName;
        if (!string.IsNullOrEmpty(updateEmployeeDto.LastName))
            employee.LastName = updateEmployeeDto.LastName;
        if (updateEmployeeDto.Email != null)
            employee.Email = updateEmployeeDto.Email;
        if (updateEmployeeDto.Phone != null)
            employee.Phone = updateEmployeeDto.Phone;
        if (updateEmployeeDto.Address != null)
            employee.Address = updateEmployeeDto.Address;
        if (updateEmployeeDto.DateOfBirth.HasValue)
            employee.DateOfBirth = updateEmployeeDto.DateOfBirth;
        if (updateEmployeeDto.TerminationDate.HasValue)
            employee.TerminationDate = updateEmployeeDto.TerminationDate;
        if (!string.IsNullOrEmpty(updateEmployeeDto.Position))
            employee.Position = updateEmployeeDto.Position;
        if (!string.IsNullOrEmpty(updateEmployeeDto.Department))
            employee.Department = updateEmployeeDto.Department;
        if (updateEmployeeDto.BaseSalary.HasValue)
            employee.BaseSalary = updateEmployeeDto.BaseSalary.Value;
        if (updateEmployeeDto.CnssNumber != null)
            employee.CnssNumber = updateEmployeeDto.CnssNumber;
        if (updateEmployeeDto.CinNumber != null)
            employee.CinNumber = updateEmployeeDto.CinNumber;
        if (updateEmployeeDto.IsActive.HasValue)
            employee.IsActive = updateEmployeeDto.IsActive.Value;

        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var companyId = GetCompanyId();
        if (companyId == 0) return Unauthorized();

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == id && e.CompanyId == companyId);

        if (employee == null)
            return NotFound();

        // Soft delete - just mark as inactive
        employee.IsActive = false;
        employee.TerminationDate = DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}