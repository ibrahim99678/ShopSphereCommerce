using Microsoft.AspNetCore.Identity.UI.Services;

namespace ShopSphere.Web.Services;

public class DevelopmentEmailSender : IEmailSender
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DevelopmentEmailSender> _logger;

    public DevelopmentEmailSender(IWebHostEnvironment environment, ILogger<DevelopmentEmailSender> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (!_environment.IsDevelopment())
        {
            throw new InvalidOperationException("Email sending is not configured.");
        }

        _logger.LogInformation("Email to {Email}. Subject: {Subject}. Body: {Body}", email, subject, htmlMessage);
        return Task.CompletedTask;
    }
}

