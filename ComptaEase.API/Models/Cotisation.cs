using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComptaEase.API.Models;

public class Cotisation
{
    public int Id { get; set; }
    
    [Required]
    public int CompanyId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public CotisationType Type { get; set; }
    
    [Column(TypeName = "decimal(5,4)")]
    public decimal EmployeeRate { get; set; }
    
    [Column(TypeName = "decimal(5,4)")]
    public decimal EmployerRate { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxAmount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinAmount { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Company Company { get; set; } = null!;
}

public enum CotisationType
{
    CNSS = 1,
    AMO = 2,
    IGR = 3,
    CIMR = 4,
    Other = 5
}