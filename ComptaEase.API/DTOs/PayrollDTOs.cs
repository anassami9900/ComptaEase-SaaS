using System.ComponentModel.DataAnnotations;

namespace ComptaEase.API.DTOs;

public class PayrollDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeFullName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public int PayrollPeriodYear { get; set; }
    public int PayrollPeriodMonth { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal CnssEmployee { get; set; }
    public decimal CnssEmployer { get; set; }
    public decimal AmoEmployee { get; set; }
    public decimal AmoEmployer { get; set; }
    public decimal IgrTax { get; set; }
    public decimal OtherDeductions { get; set; }
    public int WorkedDays { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreatePayrollDto
{
    [Required]
    public int EmployeeId { get; set; }
    
    [Required]
    [Range(2000, 3000)]
    public int PayrollPeriodYear { get; set; }
    
    [Required]
    [Range(1, 12)]
    public int PayrollPeriodMonth { get; set; }
    
    [Required]
    [Range(1, 31)]
    public int WorkedDays { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal TotalAllowances { get; set; } = 0;
    
    [Range(0, double.MaxValue)]
    public decimal OtherDeductions { get; set; } = 0;
    
    public DateTime? PaymentDate { get; set; }
}

public class PayrollCalculationDto
{
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal CnssEmployee { get; set; }
    public decimal CnssEmployer { get; set; }
    public decimal AmoEmployee { get; set; }
    public decimal AmoEmployer { get; set; }
    public decimal IgrTax { get; set; }
    public PayrollBreakdownDto Breakdown { get; set; } = new();
}

public class PayrollBreakdownDto
{
    public decimal BaseSalary { get; set; }
    public decimal Allowances { get; set; }
    public decimal GrossSalary { get; set; }
    public List<DeductionDto> Deductions { get; set; } = new();
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
}

public class DeductionDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
}