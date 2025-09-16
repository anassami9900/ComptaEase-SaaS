using System.Security.Claims;
using System.Text;
using ComptaEase.API.Data;
using ComptaEase.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ComptaEase.API.Middleware;

public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        if (ShouldAudit(context))
        {
            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                await LogAuditAsync(context, dbContext);
            }

            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        else
        {
            await _next(context);
        }
    }

    private static bool ShouldAudit(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();
        var method = context.Request.Method.ToUpper();

        // Only audit specific endpoints and methods
        if (path == null) return false;

        // Skip health checks, swagger, and static files
        if (path.Contains("/health") || path.Contains("/swagger") || path.Contains("/api-docs"))
            return false;

        // Skip GET requests for most endpoints (except sensitive ones)
        if (method == "GET" && !path.Contains("/payroll"))
            return false;

        // Audit all POST, PUT, DELETE operations
        return method is "POST" or "PUT" or "DELETE" or "PATCH";
    }

    private async Task LogAuditAsync(HttpContext context, AppDbContext dbContext)
    {
        try
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var companyIdClaim = context.User.FindFirst("CompanyId")?.Value;

            if (string.IsNullOrEmpty(userId) || !int.TryParse(companyIdClaim, out var companyId))
                return;

            var auditLog = new AuditLog
            {
                CompanyId = companyId,
                UserId = userId,
                Action = context.Request.Method,
                EntityType = ExtractEntityType(context.Request.Path),
                EntityId = ExtractEntityId(context.Request.Path),
                IpAddress = GetClientIpAddress(context),
                UserAgent = context.Request.Headers.UserAgent.FirstOrDefault(),
                Timestamp = DateTime.UtcNow
            };

            dbContext.AuditLogs.Add(auditLog);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit entry");
        }
    }

    private static string ExtractEntityType(PathString path)
    {
        var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments != null && segments.Length >= 2)
        {
            return segments[1]; // Assumes /api/{entityType} pattern
        }
        return "Unknown";
    }

    private static string? ExtractEntityId(PathString path)
    {
        var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments != null && segments.Length >= 3 && int.TryParse(segments[2], out _))
        {
            return segments[2];
        }
        return null;
    }

    private static string? GetClientIpAddress(HttpContext context)
    {
        var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        }
        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = context.Connection.RemoteIpAddress?.ToString();
        }
        return ipAddress;
    }
}