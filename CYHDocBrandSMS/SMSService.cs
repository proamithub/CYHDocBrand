namespace CYHDocBrandSMS;

public interface ISMSService
{
    Task<bool> SendSmsAsync(string phoneNumber, string message);
}

public class SMSService : ISMSService
{
    public Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        return Task.FromResult(true);
    }
}
