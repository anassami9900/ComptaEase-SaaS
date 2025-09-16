using ComptaEase.API.DTOs;
using ComptaEase.API.Models;

namespace ComptaEase.API.Services;

public interface IPayrollService
{
    Task<PayrollCalculationDto> CalculatePayrollAsync(Employee employee, int workedDays, decimal allowances, decimal otherDeductions, int companyId);
    Task<PayrollDto?> CreatePayrollAsync(CreatePayrollDto createPayrollDto, int companyId);
    Task<IEnumerable<PayrollDto>> GetPayrollsAsync(int companyId, int? year = null, int? month = null);
    Task<PayrollDto?> GetPayrollByIdAsync(int id, int companyId);
    Task<bool> ApprovePayrollAsync(int id, int companyId);
}