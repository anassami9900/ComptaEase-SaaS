using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ComptaEase.API.Data;
using ComptaEase.API.DTOs;
using ComptaEase.API.Models;
using ComptaEase.API.Services;

namespace ComptaEase.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PayrollController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPayrollService _payrollService;
    private readonly IPdfService _pdfService;
    private readonly IEmailService _emailService;
    private readonly ILogger<PayrollController> _logger;

    public PayrollController(
        AppDbContext context,
        IPayrollService payrollService,
        IPdfService pdfService,
        IEmailService emailService,
        ILogger<PayrollController> logger)
    {
        _context = context;
        _payrollService = payrollService;
        _pdfService = pdfService;
        _emailService = emailService;
        _logger = logger;
    }

    private int GetCompanyId()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        return int.TryParse(companyIdClaim, out var companyId) ? companyId : 0;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PayrollDto>>> GetPayrolls([FromQuery] int? year, [FromQuery] int? month)
    {
        var companyId = GetCompanyId();
        if (companyId == 0) return Unauthorized();

        var payrolls = await _payrollService.GetPayrollsAsync(companyId, year, month);
        return Ok(payrolls);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PayrollDto>> GetPayroll(int id)
    {
        var companyId = GetCompanyId();
        if (companyId == 0) return Unauthorized();

        var payroll = await _payrollService.GetPayrollByIdAsync(id, companyId);
        if (payroll == null)
            return NotFound();

        return Ok(payroll);
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<PayrollCalculationDto>> CalculatePayroll([FromBody] CalculatePayrollDto calculateDto)
    {
        var companyId = GetCompanyId();
        if (companyId == 0) return Unauthorized();

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == calculateDto.EmployeeId && e.CompanyId == companyId);

        if (employee == null)
            return NotFound(new { message = "Employee not found" });

        var calculation = await _payrollService.CalculatePayrollAsync(
            employee,
            calculateDto.WorkedDays,
            calculateDto.Allowances,
            calculateDto.OtherDeductions,
            companyId);

        return Ok(calculation);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<PayrollDto>> CreatePayroll([FromBody] CreatePayrollDto createPayrollDto)
    {
        var companyId = GetCompanyId();
        if (companyId == 0) return Unauthorized();

        var payroll = await _payrollService.CreatePayrollAsync(createPayrollDto, companyId);
        if (payroll == null)
            return BadRequest(new { message = "Failed to create payroll. It may already exist for this period." });

        return CreatedAtAction(nameof(GetPayroll), new { id = payroll.Id }, payroll);
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> ApprovePayroll(int id)
    {
        var companyId = GetCompanyId();
        if (companyId == 0) return Unauthorized();

        var success = await _payrollService.ApprovePayrollAsync(id, companyId);
        if (!success)
            return BadRequest(new { message = "Failed to approve payroll" });

        return NoContent();
    }

    [HttpPost("{id}/generate-bulletin")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> GenerateBulletin(int id, [FromQuery] bool sendEmail = false)
    {
        var companyId = GetCompanyId();
        if (companyId == 0) return Unauthorized();

        var payroll = await _context.Payrolls
            .Include(p => p.Employee)
            .Include(p => p.Company)
            .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

        if (payroll == null)
            return NotFound();

        try
        {
            // Generate PDF
            var filePath = await _pdfService.GeneratePayslipAsync(payroll, payroll.Employee, payroll.Company);

            // Save bulletin record
            var fileName = Path.GetFileName(filePath);
            var relativePath = $"bulletins/{fileName}";

            var bulletin = new BulletinPaie
            {
                CompanyId = companyId,
                EmployeeId = payroll.EmployeeId,
                PayrollId = payroll.Id,
                FileName = fileName,
                FilePath = relativePath,
                CreatedAt = DateTime.UtcNow
            };

            // Check if bulletin already exists
            var existingBulletin = await _context.BulletinPaies
                .FirstOrDefaultAsync(b => b.PayrollId == payroll.Id);

            if (existingBulletin != null)
            {
                existingBulletin.FileName = fileName;
                existingBulletin.FilePath = relativePath;
            }
            else
            {
                _context.BulletinPaies.Add(bulletin);
            }

            await _context.SaveChangesAsync();

            // Send email if requested
            if (sendEmail && !string.IsNullOrEmpty(payroll.Employee.Email))
            {
                var emailSent = await _emailService.SendPayslipAsync(
                    payroll.Employee.Email,
                    $"{payroll.Employee.FirstName} {payroll.Employee.LastName}",
                    filePath,
                    payroll.PayrollPeriodMonth,
                    payroll.PayrollPeriodYear);

                if (emailSent)
                {
                    var bulletinToUpdate = existingBulletin ?? bulletin;
                    bulletinToUpdate.IsEmailSent = true;
                    bulletinToUpdate.EmailSentAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(new { message = "Bulletin generated successfully", filePath = relativePath, emailSent = sendEmail });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating bulletin for payroll {PayrollId}", id);
            return StatusCode(500, new { message = "Error generating bulletin" });
        }
    }

    [HttpGet("{id}/bulletin")]
    public async Task<IActionResult> GetBulletin(int id)
    {
        var companyId = GetCompanyId();
        if (companyId == 0) return Unauthorized();

        var bulletin = await _context.BulletinPaies
            .Include(b => b.Employee)
            .FirstOrDefaultAsync(b => b.PayrollId == id && b.CompanyId == companyId);

        if (bulletin == null)
            return NotFound(new { message = "Bulletin not found" });

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", bulletin.FilePath);
        
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { message = "Bulletin file not found" });

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(fileBytes, "application/pdf", bulletin.FileName);
    }

    [HttpPost("{id}/send-email")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> SendBulletinEmail(int id)
    {
        var companyId = GetCompanyId();
        if (companyId == 0) return Unauthorized();

        var bulletin = await _context.BulletinPaies
            .Include(b => b.Employee)
            .Include(b => b.Payroll)
            .FirstOrDefaultAsync(b => b.PayrollId == id && b.CompanyId == companyId);

        if (bulletin == null)
            return NotFound(new { message = "Bulletin not found" });

        if (string.IsNullOrEmpty(bulletin.Employee.Email))
            return BadRequest(new { message = "Employee email is not available" });

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", bulletin.FilePath);
        
        if (!System.IO.File.Exists(filePath))
            return NotFound(new { message = "Bulletin file not found" });

        try
        {
            var emailSent = await _emailService.SendPayslipAsync(
                bulletin.Employee.Email,
                $"{bulletin.Employee.FirstName} {bulletin.Employee.LastName}",
                filePath,
                bulletin.Payroll.PayrollPeriodMonth,
                bulletin.Payroll.PayrollPeriodYear);

            if (emailSent)
            {
                bulletin.IsEmailSent = true;
                bulletin.EmailSentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Email sent successfully" });
            }
            else
            {
                return StatusCode(500, new { message = "Failed to send email" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulletin email for payroll {PayrollId}", id);
            return StatusCode(500, new { message = "Error sending email" });
        }
    }
}

public class CalculatePayrollDto
{
    public int EmployeeId { get; set; }
    public int WorkedDays { get; set; } = 30;
    public decimal Allowances { get; set; } = 0;
    public decimal OtherDeductions { get; set; } = 0;
}