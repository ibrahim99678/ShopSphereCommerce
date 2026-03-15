using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ShopSphere.Web.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public SmtpEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var host = _configuration["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new InvalidOperationException("SMTP is not configured.");
        }

        var port = int.TryParse(_configuration["Smtp:Port"], out var p) ? p : 587;
        var from = _configuration["Smtp:From"];
        if (string.IsNullOrWhiteSpace(from))
        {
            from = _configuration["Smtp:User"];
        }

        if (string.IsNullOrWhiteSpace(from))
        {
            throw new InvalidOperationException("SMTP 'From' address is not configured.");
        }

        var enableSsl = !string.Equals(_configuration["Smtp:EnableSsl"], "false", StringComparison.OrdinalIgnoreCase);
        var user = _configuration["Smtp:User"];
        var pass = _configuration["Smtp:Pass"];

        using var message = new MailMessage(from, email, subject, htmlMessage)
        {
            IsBodyHtml = true
        };

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl
        };

        if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
        {
            client.Credentials = new NetworkCredential(user, pass);
        }

        await client.SendMailAsync(message);
    }
}

