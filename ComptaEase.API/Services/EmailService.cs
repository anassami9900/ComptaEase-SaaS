using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ComptaEase.API.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendPayslipAsync(string recipientEmail, string recipientName, string filePath, int month, int year)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var senderEmail = _configuration["Email:SenderEmail"];
            var senderPassword = _configuration["Email:SenderPassword"];
            var senderName = _configuration["Email:SenderName"] ?? "ComptaEase SaaS";

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
            {
                _logger.LogWarning("Email configuration is incomplete. Skipping email send.");
                return false;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress(recipientName, recipientEmail));
            message.Subject = $"Bulletin de paie - {month:D2}/{year}";

            var builder = new BodyBuilder
            {
                HtmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Bonjour {recipientName},</h2>
                        <p>Veuillez trouver ci-joint votre bulletin de paie pour la période {month:D2}/{year}.</p>
                        <p>Si vous avez des questions concernant votre bulletin de paie, n'hésitez pas à contacter le service RH.</p>
                        <br>
                        <p>Cordialement,<br>
                        L'équipe ComptaEase</p>
                    </body>
                    </html>"
            };

            if (File.Exists(filePath))
            {
                builder.Attachments.Add(filePath);
            }

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(senderEmail, senderPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation($"Payslip email sent successfully to {recipientEmail}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send payslip email to {recipientEmail}");
            return false;
        }
    }
}