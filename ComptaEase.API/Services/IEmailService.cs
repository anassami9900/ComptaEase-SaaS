namespace ComptaEase.API.Services;

public interface IEmailService
{
    Task<bool> SendPayslipAsync(string recipientEmail, string recipientName, string filePath, int month, int year);
}