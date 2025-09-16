using ComptaEase.API.Models;

namespace ComptaEase.API.Services;

public interface IPdfService
{
    Task<string> GeneratePayslipAsync(Payroll payroll, Employee employee, Company company);
}