using System.ComponentModel.DataAnnotations;

namespace ComptaEase.API.DTOs;

public class EmployeeDto
{
    public int Id { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string Position { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal BaseSalary { get; set; }
    public string? CnssNumber { get; set; }
    public string? CinNumber { get; set; }
    public bool IsActive { get; set; }
}

public class CreateEmployeeDto
{
    [Required]
    public string EmployeeNumber { get; set; } = string.Empty;
    
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    [EmailAddress]
    public string? Email { get; set; }
    
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    
    [Required]
    public DateTime HireDate { get; set; }
    
    [Required]
    public string Position { get; set; } = string.Empty;
    
    [Required]
    public string Department { get; set; } = string.Empty;
    
    [Required]
    [Range(0, double.MaxValue)]
    public decimal BaseSalary { get; set; }
    
    public string? CnssNumber { get; set; }
    public string? CinNumber { get; set; }
}

public class UpdateEmployeeDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string? Position { get; set; }
    public string? Department { get; set; }
    public decimal? BaseSalary { get; set; }
    public string? CnssNumber { get; set; }
    public string? CinNumber { get; set; }
    public bool? IsActive { get; set; }
}