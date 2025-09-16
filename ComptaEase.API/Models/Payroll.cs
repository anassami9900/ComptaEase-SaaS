using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComptaEase.API.Models;

public class Payroll
{
    public int Id { get; set; }
    
    [Required]
    public int CompanyId { get; set; }
    
    [Required]
    public int EmployeeId { get; set; }
    
    [Required]
    public int PayrollPeriodYear { get; set; }
    
    [Required]
    public int PayrollPeriodMonth { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal GrossSalary { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal NetSalary { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDeductions { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAllowances { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal CnssEmployee { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal CnssEmployer { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal AmoEmployee { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal AmoEmployer { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal IgrTax { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal OtherDeductions { get; set; }
    
    public int WorkedDays { get; set; }
    
    public DateTime PaymentDate { get; set; }
    
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Company Company { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
    public ICollection<BulletinPaie> BulletinPaies { get; set; } = new List<BulletinPaie>();
}

public enum PayrollStatus
{
    Draft = 1,
    Approved = 2,
    Paid = 3,
    Cancelled = 4
}