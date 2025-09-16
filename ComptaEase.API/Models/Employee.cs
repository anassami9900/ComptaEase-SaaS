using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComptaEase.API.Models;

public class Employee
{
    public int Id { get; set; }
    
    [Required]
    public int CompanyId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string EmployeeNumber { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Email { get; set; }
    
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [StringLength(200)]
    public string? Address { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    public DateTime HireDate { get; set; }
    
    public DateTime? TerminationDate { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Position { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Department { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal BaseSalary { get; set; }
    
    [StringLength(20)]
    public string? CnssNumber { get; set; }
    
    [StringLength(20)]
    public string? CinNumber { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Company Company { get; set; } = null!;
    public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
    public ICollection<BulletinPaie> BulletinPaies { get; set; } = new List<BulletinPaie>();
}