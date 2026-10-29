namespace CYHDocBrandSMS;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody, string? textBody = null);
    Task<bool> SendVerificationLinkEmailAsync(string toEmail, string doctorName, string verificationUrl);
}
