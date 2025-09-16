using System.ComponentModel.DataAnnotations;

namespace ComptaEase.API.Models;

public class BulletinPaie
{
    public int Id { get; set; }
    
    [Required]
    public int CompanyId { get; set; }
    
    [Required]
    public int EmployeeId { get; set; }
    
    [Required]
    public int PayrollId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    public bool IsEmailSent { get; set; } = false;
    
    public DateTime? EmailSentAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Company Company { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
    public Payroll Payroll { get; set; } = null!;
}