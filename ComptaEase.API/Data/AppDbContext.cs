using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ComptaEase.API.Models;

namespace ComptaEase.API.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Cotisation> Cotisations { get; set; }
    public DbSet<Payroll> Payrolls { get; set; }
    public DbSet<BulletinPaie> BulletinPaies { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Company
        builder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure User
        builder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.CompanyId);
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Users)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure Employee
        builder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CompanyId, e.EmployeeNumber }).IsUnique();
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Employees)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure Cotisation
        builder.Entity<Cotisation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CompanyId);
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Cotisations)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure Payroll
        builder.Entity<Payroll>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CompanyId, e.EmployeeId, e.PayrollPeriodYear, e.PayrollPeriodMonth }).IsUnique();
            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Payrolls)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Employee)
                  .WithMany(emp => emp.Payrolls)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure BulletinPaie
        builder.Entity<BulletinPaie>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CompanyId);
            entity.HasIndex(e => e.PayrollId).IsUnique();
            entity.HasOne(e => e.Company)
                  .WithMany()
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Employee)
                  .WithMany(emp => emp.BulletinPaies)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Payroll)
                  .WithMany(p => p.BulletinPaies)
                  .HasForeignKey(e => e.PayrollId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure AuditLog
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CompanyId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasOne(e => e.Company)
                  .WithMany()
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.AuditLogs)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}