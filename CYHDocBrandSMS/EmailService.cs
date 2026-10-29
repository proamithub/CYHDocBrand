using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CYHDocBrandSMS;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<bool> SendVerificationLinkEmailAsync(string toEmail, string doctorName, string verificationUrl)
    {
        var subject = "Verify your Email Address — CYH Portal";
        var safeName = string.IsNullOrWhiteSpace(doctorName) ? "Doctor" : doctorName.Trim();
        if (!safeName.StartsWith("Dr.", StringComparison.OrdinalIgnoreCase))
        {
            safeName = "Dr. " + safeName;
        }

        var htmlBody = BuildVerificationHtmlTemplate(safeName, verificationUrl);
        var textBody = $"Hello {safeName},\n\nPlease verify your official email address on CYH Portal by clicking the link below:\n{verificationUrl}\n\nThis verification link is valid for 24 hours.\n\nThank you,\nCYH Portal Team";

        return await SendEmailAsync(toEmail, subject, htmlBody, textBody);
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, string? textBody = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                return false;

            var smtpHost = _config["EmailGateway:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.TryParse(_config["EmailGateway:SmtpPort"], out var port) ? port : 587;
            var enableSsl = !bool.TryParse(_config["EmailGateway:EnableSsl"], out var ssl) || ssl;
            var senderEmail = _config["EmailGateway:SenderEmail"] ?? "no-reply@clickyourhospital.com";
            var senderName = _config["EmailGateway:SenderName"] ?? "CYH Portal";
            var username = _config["EmailGateway:Username"] ?? "";
            var password = _config["EmailGateway:Password"] ?? "";
            var isLive = bool.TryParse(_config["EmailGateway:IsLive"], out var live) && live;

            _logger.LogInformation("📧 [EMAIL GATEWAY REQUEST] To: {To} | Subject: {Sub} | IsLive: {IsLive} | Host: {Host}:{Port}", 
                toEmail, subject, isLive, smtpHost, smtpPort);

            // Simulation / Dev Fallback Mode (if IsLive is false or credentials not set)
            if (!isLive || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogInformation("📢 [EMAIL GATEWAY SIMULATION] (Live disabled or credentials not set).\nTo: {To}\nSubject: {Sub}\nBody Preview:\n{Body}", 
                    toEmail, subject, textBody ?? "HTML Email");
                return true;
            }

            // Real SMTP Transmission
            using var message = new MailMessage();
            message.From = new MailAddress(senderEmail, senderName, Encoding.UTF8);
            message.To.Add(new MailAddress(toEmail.Trim()));
            message.Subject = subject;
            message.SubjectEncoding = Encoding.UTF8;
            message.Body = htmlBody;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;

            if (!string.IsNullOrWhiteSpace(textBody))
            {
                var plainView = AlternateView.CreateAlternateViewFromString(textBody, Encoding.UTF8, "text/plain");
                var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html");
                message.AlternateViews.Add(plainView);
                message.AlternateViews.Add(htmlView);
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, password),
                Timeout = 15000
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("✅ [EMAIL GATEWAY SUCCESS] Successfully sent email to {To}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [EMAIL GATEWAY ERROR] Failed to send email to {To}", toEmail);
            return false;
        }
    }

    private static string BuildVerificationHtmlTemplate(string doctorName, string verificationUrl)
    {
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""UTF-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
<title>Verify your Email — CYH Portal</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #070d1e; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #ffffff;"">
  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #070d1e; padding: 40px 15px;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 580px; background: #0c1833; border: 1px solid rgba(0, 242, 254, 0.25); border-radius: 18px; box-shadow: 0 20px 45px rgba(0,0,0,0.6); overflow: hidden;"">
          <!-- Top Accent Line -->
          <tr>
            <td style=""height: 4px; background: linear-gradient(90deg, #00f2fe, #4facfe, #8b5cf6);""></td>
          </tr>
          <!-- Header -->
          <tr>
            <td align=""center"" style=""padding: 35px 30px 20px 30px;"">
              <div style=""display: inline-block; padding: 8px 18px; background: rgba(0, 242, 254, 0.1); border: 1px solid rgba(0, 242, 254, 0.3); border-radius: 50px; color: #00f2fe; font-size: 12px; font-weight: 700; letter-spacing: 1px; text-transform: uppercase; margin-bottom: 15px;"">
                Official Verification
              </div>
              <h1 style=""margin: 0; font-size: 24px; font-weight: 700; color: #ffffff; letter-spacing: -0.5px;"">CYH Portal</h1>
              <p style=""margin: 6px 0 0 0; color: #94a3b8; font-size: 13px;"">Care Aisi Family Jaisi</p>
            </td>
          </tr>
          <!-- Body Content -->
          <tr>
            <td style=""padding: 20px 35px 30px 35px;"">
              <h2 style=""margin: 0 0 14px 0; font-size: 19px; font-weight: 600; color: #ffffff;"">Hello {doctorName},</h2>
              <p style=""margin: 0 0 20px 0; color: #cbd5e1; font-size: 15px; line-height: 1.6;"">
                Thank you for joining <strong>CYH Portal</strong>. Please confirm your official email address to complete your account verification and activate all specialist features.
              </p>
              
              <!-- Verification Button -->
              <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""margin: 30px 0;"">
                <tr>
                  <td align=""center"">
                    <a href=""{verificationUrl}"" target=""_blank"" style=""display: inline-block; background: linear-gradient(135deg, #00f2fe 0%, #4facfe 100%); color: #070d1e; font-size: 16px; font-weight: 700; text-decoration: none; padding: 14px 38px; border-radius: 12px; box-shadow: 0 6px 20px rgba(0, 242, 254, 0.4); text-align: center; letter-spacing: 0.3px;"">
                      ✓ Verify Email Address
                    </a>
                  </td>
                </tr>
              </table>

              <p style=""margin: 25px 0 10px 0; color: #94a3b8; font-size: 13px; line-height: 1.5;"">
                Or copy and paste this link into your browser:
              </p>
              <div style=""background: rgba(15, 23, 42, 0.7); border: 1px solid rgba(255, 255, 255, 0.1); border-radius: 8px; padding: 10px 14px; word-break: break-all; font-size: 12px; color: #38bdf8; font-family: monospace;"">
                {verificationUrl}
              </div>

              <div style=""margin-top: 28px; padding-top: 20px; border-top: 1px solid rgba(255, 255, 255, 0.08); color: #64748b; font-size: 12px; line-height: 1.5;"">
                ⏱ <strong>Link Expiry:</strong> This link is valid for <strong>24 hours</strong>.<br/>
                🔒 If you did not request this email, you can safely ignore it.
              </div>
            </td>
          </tr>
          <!-- Footer -->
          <tr>
            <td align=""center"" style=""padding: 20px 30px; background: rgba(5, 10, 24, 0.7); border-top: 1px solid rgba(255, 255, 255, 0.06); color: #64748b; font-size: 12px;"">
              © {DateTime.UtcNow.Year} Click Your Hospital (CYH). All rights reserved.<br/>
              Professional Healthcare Branding &amp; Specialist Portal
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }
}
