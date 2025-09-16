using System.ComponentModel.DataAnnotations;

namespace ComptaEase.API.Models;

public class Company
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? Address { get; set; }
    
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [StringLength(100)]
    public string? Email { get; set; }
    
    [StringLength(50)]
    public string? TaxId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<Cotisation> Cotisations { get; set; } = new List<Cotisation>();
    public ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
}