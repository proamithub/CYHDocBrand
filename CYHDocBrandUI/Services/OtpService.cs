using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using CYHDocBrandModel.DTO;
using CYHDocBrandRepo.IRepository;
using CYHDocBrandSMS;

namespace CYHDocBrandUI.Services;

public class OtpSendResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
    public string? MaskedPhone { get; set; }
    public int ExpirySeconds { get; set; } = 300;
}

public class OtpVerifyResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public CurrentUserDTO? User { get; set; }
    public bool IsDemoLogin { get; set; }
}

public interface IOtpService
{
    Task<OtpSendResult> SendOtpAsync(string phoneNumber);
    Task<OtpVerifyResult> VerifyOtpAsync(string phoneNumber, string otp);
}

public class OtpService : IOtpService
{
    private readonly ISMSService _smsService;
    private readonly IMemoryCache _memoryCache;
    private readonly IDoctorRepository _doctorRepo;
    private readonly ILogger<OtpService> _logger;

    public const string DemoOtp = "123456";

    public OtpService(
        ISMSService smsService,
        IMemoryCache memoryCache,
        IDoctorRepository doctorRepo,
        ILogger<OtpService> logger)
    {
        _smsService = smsService;
        _memoryCache = memoryCache;
        _doctorRepo = doctorRepo;
        _logger = logger;
    }

    public async Task<OtpSendResult> SendOtpAsync(string phoneNumber)
    {
        var cleanPhone = CleanPhoneNumber(phoneNumber);
        if (cleanPhone.Length != 10)
        {
            return new OtpSendResult
            {
                Success = false,
                ErrorMessage = "Please enter a valid 10-digit mobile number."
            };
        }

        // Generate 6-digit cryptographic random OTP
        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString("D6");
        var cacheKey = $"CYH_OTP_{cleanPhone}";

        // Cache for 5 minutes
        _memoryCache.Set(cacheKey, otp, TimeSpan.FromMinutes(5));

        _logger.LogInformation("🔑 [OTP CREATED] Phone: {Phone} | Real OTP: {Otp} | Demo OTP: {DemoOtp}", cleanPhone, otp, DemoOtp);

        // Send SMS via real SMS Gateway
        await _smsService.SendOtpSmsAsync(cleanPhone, otp);

        var masked = $"+91 {cleanPhone.Substring(0, 2)}******{cleanPhone.Substring(6, 4)}";

        return new OtpSendResult
        {
            Success = true,
            MaskedPhone = masked,
            Message = $"OTP has been sent to {masked}. (For Demo/Testing, enter 123456)",
            ExpirySeconds = 300
        };
    }

    public async Task<OtpVerifyResult> VerifyOtpAsync(string phoneNumber, string otp)
    {
        var cleanPhone = CleanPhoneNumber(phoneNumber);
        var cleanOtp = otp?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(cleanOtp) || cleanOtp.Length < 6)
        {
            return new OtpVerifyResult
            {
                Success = false,
                ErrorMessage = "Please enter the 6-digit OTP code."
            };
        }

        var isDemo = string.Equals(cleanOtp, DemoOtp, StringComparison.Ordinal);
        var cacheKey = $"CYH_OTP_{cleanPhone}";
        var isRealValid = _memoryCache.TryGetValue(cacheKey, out string? cachedOtp) &&
                          string.Equals(cachedOtp, cleanOtp, StringComparison.Ordinal);

        if (!isDemo && !isRealValid)
        {
            return new OtpVerifyResult
            {
                Success = false,
                ErrorMessage = $"Invalid or expired OTP. Please re-enter, or use demo OTP: {DemoOtp}"
            };
        }

        // Clear cached OTP on successful verification
        _memoryCache.Remove(cacheKey);

        // Fetch User by phone
        var user = await _doctorRepo.GetByPhoneAsync(cleanPhone);

        // If not found in database by phone, provide intelligent demo fallback:
        if (user == null)
        {
            if (cleanPhone == "9876543210")
            {
                // Demo Admin
                user = await _doctorRepo.GetByEmailAsync("admin@cyhdoctorhub.com");
            }
            else
            {
                // Demo Doctor fallback so testing with any number + 123456 succeeds
                user = await _doctorRepo.GetByEmailAsync("doctor@cyhdoctorhub.com");
                if (user == null)
                {
                    var allDocs = await _doctorRepo.GetAllWithUsersAsync();
                    var firstDoc = allDocs.FirstOrDefault();
                    if (firstDoc != null)
                    {
                        user = new CurrentUserDTO
                        {
                            UserId = firstDoc.UserId,
                            FullName = firstDoc.FullName,
                            Email = firstDoc.User?.Email ?? "doctor@cyhdoctorhub.com",
                            Role = firstDoc.User?.Role ?? "Doctor",
                            Department = firstDoc.Specialization,
                            DoctorProfileId = firstDoc.Id,
                            AvatarUrl = firstDoc.ProfilePic ?? firstDoc.User?.AvatarUrl,
                            IsActive = firstDoc.User?.IsActive ?? true
                        };
                    }
                }
            }
        }

        if (user == null)
        {
            return new OtpVerifyResult
            {
                Success = false,
                ErrorMessage = "No active doctor or admin profile found for this mobile number."
            };
        }

        return new OtpVerifyResult
        {
            Success = true,
            User = user,
            IsDemoLogin = isDemo
        };
    }

    private static string CleanPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length == 12 && digits.StartsWith("91")) return digits.Substring(2);
        if (digits.Length == 11 && digits.StartsWith("0")) return digits.Substring(1);
        if (digits.Length > 10) return digits.Substring(digits.Length - 10);
        return digits;
    }
}
