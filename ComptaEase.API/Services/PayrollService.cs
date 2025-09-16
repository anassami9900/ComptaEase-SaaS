using Microsoft.EntityFrameworkCore;
using ComptaEase.API.Data;
using ComptaEase.API.DTOs;
using ComptaEase.API.Models;

namespace ComptaEase.API.Services;

public class PayrollService : IPayrollService
{
    private readonly AppDbContext _context;

    public PayrollService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PayrollCalculationDto> CalculatePayrollAsync(Employee employee, int workedDays, decimal allowances, decimal otherDeductions, int companyId)
    {
        // Get company cotisations
        var cotisations = await _context.Cotisations
            .Where(c => c.CompanyId == companyId && c.IsActive)
            .ToListAsync();

        // Calculate base salary for worked days (assuming 30 days per month)
        var dailySalary = employee.BaseSalary / 30;
        var baseSalaryForPeriod = dailySalary * workedDays;
        var grossSalary = baseSalaryForPeriod + allowances;

        // Calculate CNSS (employee and employer)
        var cnssRate = cotisations.FirstOrDefault(c => c.Type == CotisationType.CNSS);
        var cnssEmployee = cnssRate != null ? Math.Min(grossSalary * cnssRate.EmployeeRate, cnssRate.MaxAmount ?? decimal.MaxValue) : 0;
        var cnssEmployer = cnssRate != null ? Math.Min(grossSalary * cnssRate.EmployerRate, cnssRate.MaxAmount ?? decimal.MaxValue) : 0;

        // Calculate AMO (employee and employer)
        var amoRate = cotisations.FirstOrDefault(c => c.Type == CotisationType.AMO);
        var amoEmployee = amoRate != null ? Math.Min(grossSalary * amoRate.EmployeeRate, amoRate.MaxAmount ?? decimal.MaxValue) : 0;
        var amoEmployer = amoRate != null ? Math.Min(grossSalary * amoRate.EmployerRate, amoRate.MaxAmount ?? decimal.MaxValue) : 0;

        // Calculate IGR (simplified calculation)
        var igrTax = CalculateIGR(grossSalary);

        var totalDeductions = cnssEmployee + amoEmployee + igrTax + otherDeductions;
        var netSalary = grossSalary - totalDeductions;

        var breakdown = new PayrollBreakdownDto
        {
            BaseSalary = baseSalaryForPeriod,
            Allowances = allowances,
            GrossSalary = grossSalary,
            Deductions = new List<DeductionDto>
            {
                new() { Name = "CNSS Employee", Amount = cnssEmployee, Type = "Social" },
                new() { Name = "AMO Employee", Amount = amoEmployee, Type = "Social" },
                new() { Name = "IGR Tax", Amount = igrTax, Type = "Tax" },
                new() { Name = "Other Deductions", Amount = otherDeductions, Type = "Other" }
            },
            TotalDeductions = totalDeductions,
            NetSalary = netSalary
        };

        return new PayrollCalculationDto
        {
            GrossSalary = grossSalary,
            NetSalary = netSalary,
            TotalDeductions = totalDeductions,
            CnssEmployee = cnssEmployee,
            CnssEmployer = cnssEmployer,
            AmoEmployee = amoEmployee,
            AmoEmployer = amoEmployer,
            IgrTax = igrTax,
            Breakdown = breakdown
        };
    }

    public async Task<PayrollDto?> CreatePayrollAsync(CreatePayrollDto createPayrollDto, int companyId)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == createPayrollDto.EmployeeId && e.CompanyId == companyId);

        if (employee == null)
            return null;

        // Check if payroll already exists for this period
        var existingPayroll = await _context.Payrolls
            .FirstOrDefaultAsync(p => p.EmployeeId == createPayrollDto.EmployeeId &&
                                     p.PayrollPeriodYear == createPayrollDto.PayrollPeriodYear &&
                                     p.PayrollPeriodMonth == createPayrollDto.PayrollPeriodMonth &&
                                     p.CompanyId == companyId);

        if (existingPayroll != null)
            return null; // Payroll already exists

        var calculation = await CalculatePayrollAsync(employee, createPayrollDto.WorkedDays, createPayrollDto.TotalAllowances, createPayrollDto.OtherDeductions, companyId);

        var payroll = new Payroll
        {
            CompanyId = companyId,
            EmployeeId = createPayrollDto.EmployeeId,
            PayrollPeriodYear = createPayrollDto.PayrollPeriodYear,
            PayrollPeriodMonth = createPayrollDto.PayrollPeriodMonth,
            GrossSalary = calculation.GrossSalary,
            NetSalary = calculation.NetSalary,
            TotalDeductions = calculation.TotalDeductions,
            TotalAllowances = createPayrollDto.TotalAllowances,
            CnssEmployee = calculation.CnssEmployee,
            CnssEmployer = calculation.CnssEmployer,
            AmoEmployee = calculation.AmoEmployee,
            AmoEmployer = calculation.AmoEmployer,
            IgrTax = calculation.IgrTax,
            OtherDeductions = createPayrollDto.OtherDeductions,
            WorkedDays = createPayrollDto.WorkedDays,
            PaymentDate = createPayrollDto.PaymentDate ?? DateTime.UtcNow.AddDays(5),
            Status = PayrollStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Payrolls.Add(payroll);
        await _context.SaveChangesAsync();

        return MapToPayrollDto(payroll, employee);
    }

    public async Task<IEnumerable<PayrollDto>> GetPayrollsAsync(int companyId, int? year = null, int? month = null)
    {
        var query = _context.Payrolls
            .Include(p => p.Employee)
            .Where(p => p.CompanyId == companyId);

        if (year.HasValue)
            query = query.Where(p => p.PayrollPeriodYear == year.Value);

        if (month.HasValue)
            query = query.Where(p => p.PayrollPeriodMonth == month.Value);

        var payrolls = await query
            .OrderByDescending(p => p.PayrollPeriodYear)
            .ThenByDescending(p => p.PayrollPeriodMonth)
            .ThenBy(p => p.Employee.FirstName)
            .ToListAsync();

        return payrolls.Select(p => MapToPayrollDto(p, p.Employee));
    }

    public async Task<PayrollDto?> GetPayrollByIdAsync(int id, int companyId)
    {
        var payroll = await _context.Payrolls
            .Include(p => p.Employee)
            .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

        return payroll != null ? MapToPayrollDto(payroll, payroll.Employee) : null;
    }

    public async Task<bool> ApprovePayrollAsync(int id, int companyId)
    {
        var payroll = await _context.Payrolls
            .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

        if (payroll == null || payroll.Status != PayrollStatus.Draft)
            return false;

        payroll.Status = PayrollStatus.Approved;
        payroll.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private static decimal CalculateIGR(decimal grossSalary)
    {
        // Simplified IGR calculation for Morocco
        // This is a basic implementation - in production, you'd implement the full tax brackets
        if (grossSalary <= 2500) return 0;
        if (grossSalary <= 4166.67m) return (grossSalary - 2500) * 0.10m;
        if (grossSalary <= 5000) return 166.67m + (grossSalary - 4166.67m) * 0.20m;
        if (grossSalary <= 6666.67m) return 333.33m + (grossSalary - 5000) * 0.30m;
        if (grossSalary <= 15000) return 833.33m + (grossSalary - 6666.67m) * 0.34m;
        return 3666.67m + (grossSalary - 15000) * 0.38m;
    }

    private static PayrollDto MapToPayrollDto(Payroll payroll, Employee employee)
    {
        return new PayrollDto
        {
            Id = payroll.Id,
            EmployeeId = payroll.EmployeeId,
            EmployeeFullName = $"{employee.FirstName} {employee.LastName}",
            EmployeeNumber = employee.EmployeeNumber,
            PayrollPeriodYear = payroll.PayrollPeriodYear,
            PayrollPeriodMonth = payroll.PayrollPeriodMonth,
            GrossSalary = payroll.GrossSalary,
            NetSalary = payroll.NetSalary,
            TotalDeductions = payroll.TotalDeductions,
            TotalAllowances = payroll.TotalAllowances,
            CnssEmployee = payroll.CnssEmployee,
            CnssEmployer = payroll.CnssEmployer,
            AmoEmployee = payroll.AmoEmployee,
            AmoEmployer = payroll.AmoEmployer,
            IgrTax = payroll.IgrTax,
            OtherDeductions = payroll.OtherDeductions,
            WorkedDays = payroll.WorkedDays,
            PaymentDate = payroll.PaymentDate,
            Status = payroll.Status.ToString(),
            CreatedAt = payroll.CreatedAt
        };
    }
}