using System.ComponentModel.DataAnnotations;

namespace ComptaEase.API.Models;

public class AuditLog
{
    public int Id { get; set; }
    
    [Required]
    public int CompanyId { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Action { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string EntityType { get; set; } = string.Empty;
    
    public string? EntityId { get; set; }
    
    public string? OldValues { get; set; }
    
    public string? NewValues { get; set; }
    
    [StringLength(45)]
    public string? IpAddress { get; set; }
    
    [StringLength(500)]
    public string? UserAgent { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Company Company { get; set; } = null!;
    public User User { get; set; } = null!;
}