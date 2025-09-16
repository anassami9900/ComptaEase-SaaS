using Microsoft.AspNetCore.Identity;
using ComptaEase.API.Models;

namespace ComptaEase.API.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if data already exists
        if (context.Companies.Any())
            return;

        // Create demo company
        var company = new Company
        {
            Name = "Demo Company SARL",
            Address = "123 Avenue Mohamed V, Casablanca",
            Phone = "+212 5 22 12 34 56",
            Email = "contact@democompany.ma",
            TaxId = "1234567890",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Companies.Add(company);
        await context.SaveChangesAsync();

        // Create demo admin user
        var adminUser = new User
        {
            UserName = "admin@democompany.ma",
            Email = "admin@democompany.ma",
            FirstName = "Admin",
            LastName = "User",
            CompanyId = company.Id,
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(adminUser, "Admin123!");

        // Create demo HR user
        var hrUser = new User
        {
            UserName = "hr@democompany.ma",
            Email = "hr@democompany.ma",
            FirstName = "HR",
            LastName = "Manager",
            CompanyId = company.Id,
            Role = UserRole.HR,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(hrUser, "Hr123!");

        // Create demo employee user
        var empUser = new User
        {
            UserName = "employee@democompany.ma",
            Email = "employee@democompany.ma",
            FirstName = "John",
            LastName = "Doe",
            CompanyId = company.Id,
            Role = UserRole.Employee,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(empUser, "Employee123!");

        // Create default cotisations for Morocco
        var cotisations = new List<Cotisation>
        {
            new()
            {
                CompanyId = company.Id,
                Name = "CNSS - Assurance Maladie",
                Type = CotisationType.CNSS,
                EmployeeRate = 0.0267m, // 2.67%
                EmployerRate = 0.0533m, // 5.33%
                MaxAmount = 649.8m, // Based on 6000 MAD ceiling
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                CompanyId = company.Id,
                Name = "AMO - Assurance Maladie Obligatoire",
                Type = CotisationType.AMO,
                EmployeeRate = 0.0226m, // 2.26%
                EmployerRate = 0.0339m, // 3.39%
                MaxAmount = null, // No ceiling
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                CompanyId = company.Id,
                Name = "IGR - Impôt Général sur le Revenu",
                Type = CotisationType.IGR,
                EmployeeRate = 0.0m, // Calculated progressively
                EmployerRate = 0.0m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Cotisations.AddRange(cotisations);

        // Create demo employees
        var employees = new List<Employee>
        {
            new()
            {
                CompanyId = company.Id,
                EmployeeNumber = "EMP001",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@democompany.ma",
                Phone = "+212 6 12 34 56 78",
                Address = "456 Rue de la Liberté, Rabat",
                DateOfBirth = new DateTime(1985, 5, 15),
                HireDate = DateTime.UtcNow.AddYears(-2),
                Position = "Software Developer",
                Department = "IT",
                BaseSalary = 8000m,
                CnssNumber = "123456789012",
                CinNumber = "AB123456",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                CompanyId = company.Id,
                EmployeeNumber = "EMP002",
                FirstName = "Sarah",
                LastName = "Johnson",
                Email = "sarah.johnson@democompany.ma",
                Phone = "+212 6 87 65 43 21",
                Address = "789 Boulevard Hassan II, Casablanca",
                DateOfBirth = new DateTime(1990, 8, 22),
                HireDate = DateTime.UtcNow.AddYears(-1),
                Position = "Marketing Manager",
                Department = "Marketing",
                BaseSalary = 10000m,
                CnssNumber = "123456789013",
                CinNumber = "CD789012",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                CompanyId = company.Id,
                EmployeeNumber = "EMP003",
                FirstName = "Ahmed",
                LastName = "Bennani",
                Email = "ahmed.bennani@democompany.ma",
                Phone = "+212 6 11 22 33 44",
                Address = "321 Avenue Mohammed VI, Marrakech",
                DateOfBirth = new DateTime(1988, 3, 10),
                HireDate = DateTime.UtcNow.AddMonths(-6),
                Position = "Sales Representative",
                Department = "Sales",
                BaseSalary = 6000m,
                CnssNumber = "123456789014",
                CinNumber = "EF345678",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Employees.AddRange(employees);
        await context.SaveChangesAsync();
    }
}