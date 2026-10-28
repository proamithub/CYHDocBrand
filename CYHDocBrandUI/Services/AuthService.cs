using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using CYHDocBrandData.Entities;
using CYHDocBrandModel.DTO;
using CYHDocBrandRepo.IRepository;

namespace CYHDocBrandUI.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public CurrentUserDTO? CurrentUser { get; private set; }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_currentUser));
    }

    public void SetUser(CurrentUserDTO user)
    {
        bool isSameUser = CurrentUser != null &&
            CurrentUser.UserId == user.UserId &&
            CurrentUser.Role == user.Role &&
            CurrentUser.AvatarUrl == user.AvatarUrl &&
            CurrentUser.FullName == user.FullName &&
            CurrentUser.IsActive == user.IsActive;

        CurrentUser = user;

        if (isSameUser)
        {
            return;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("DoctorProfileId", user.DoctorProfileId?.ToString() ?? ""),
            new("IsActive", user.IsActive.ToString())
        };

        if (!string.IsNullOrEmpty(user.Department))
        {
            claims.Add(new Claim("Department", user.Department));
        }

        var identity = new ClaimsIdentity(claims, "CustomAuth");
        _currentUser = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void MarkUserAsLoggedOut()
    {
        CurrentUser = null;
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}

public class LoginResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public CurrentUserDTO? User { get; set; }
}

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string email, string password);
    Task<LoginResult> LoginWithPhoneOtpAsync(string phoneNumber, string otp);
    Task LogoutAsync();
    CurrentUserDTO? GetCurrentUser();
    Task<CurrentUserDTO?> GetOrRestoreCurrentUserAsync();
}

public class AuthService : IAuthService
{
    private readonly IDoctorRepository _doctorRepo;
    private readonly IOtpService _otpService;
    private readonly CustomAuthenticationStateProvider _authStateProvider;
    private readonly IJSRuntime _js;

    public AuthService(
        IDoctorRepository doctorRepo,
        IOtpService otpService,
        CustomAuthenticationStateProvider authStateProvider,
        IJSRuntime js)
    {
        _doctorRepo = doctorRepo;
        _otpService = otpService;
        _authStateProvider = authStateProvider;
        _js = js;
    }

    public async Task<LoginResult> LoginWithPhoneOtpAsync(string phoneNumber, string otp)
    {
        var verifyResult = await _otpService.VerifyOtpAsync(phoneNumber, otp);
        if (!verifyResult.Success || verifyResult.User == null)
        {
            return new LoginResult
            {
                Success = false,
                ErrorMessage = verifyResult.ErrorMessage ?? "OTP verification failed."
            };
        }

        var user = verifyResult.User;
        _authStateProvider.SetUser(user);

        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "cyh_active_user_id", user.UserId);
            await _js.InvokeVoidAsync("localStorage.setItem", "cyh_active_user_email", user.Email);
            await _js.InvokeVoidAsync("localStorage.setItem", "cyh_active_user_role", user.Role);
        }
        catch
        {
            // Ignore
        }

        return new LoginResult
        {
            Success = true,
            User = user
        };
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return new LoginResult { Success = false, ErrorMessage = "Email and password are required." };

        // 1. Authenticate directly via SQL Stored Procedure / Query
        var user = await _doctorRepo.AuthenticateAsync(email.Trim(), password);

        if (user == null)
        {
            return new LoginResult { Success = false, ErrorMessage = "Invalid credentials. Please verify your email and password." };
        }

        _authStateProvider.SetUser(user);

        // 2. Persist session to localStorage so browser reload (F5) maintains the exact same doctor
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "cyh_active_user_id", user.UserId);
            await _js.InvokeVoidAsync("localStorage.setItem", "cyh_active_user_email", user.Email);
            await _js.InvokeVoidAsync("localStorage.setItem", "cyh_active_user_role", user.Role);
        }
        catch
        {
            // Ignore if JS storage inaccessible during static operations
        }

        return new LoginResult
        {
            Success = true,
            User = user
        };
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "cyh_active_user_id");
            await _js.InvokeVoidAsync("localStorage.removeItem", "cyh_active_user_email");
            await _js.InvokeVoidAsync("localStorage.removeItem", "cyh_active_user_role");
        }
        catch
        {
            // Ignore
        }

        _authStateProvider.MarkUserAsLoggedOut();
    }

    public CurrentUserDTO? GetCurrentUser() => _authStateProvider.CurrentUser;

    public async Task<CurrentUserDTO?> GetOrRestoreCurrentUserAsync()
    {
        // 1. Return in-memory user if active in current circuit
        if (_authStateProvider.CurrentUser != null)
        {
            return _authStateProvider.CurrentUser;
        }

        // 2. Circuit was re-created (F5 / Refresh). Restore session from browser localStorage
        try
        {
            var savedUserId = await _js.InvokeAsync<string?>("localStorage.getItem", "cyh_active_user_id");
            var savedEmail = await _js.InvokeAsync<string?>("localStorage.getItem", "cyh_active_user_email");
            var savedRole = await _js.InvokeAsync<string?>("localStorage.getItem", "cyh_active_user_role");

            if (!string.IsNullOrWhiteSpace(savedUserId))
            {
                // First attempt: match doctor by exact UserId
                var doc = await _doctorRepo.GetByUserIdAsync(savedUserId);
                if (doc != null)
                {
                    var restoredDocUser = new CurrentUserDTO
                    {
                        UserId = doc.UserId,
                        FullName = doc.FullName,
                        Email = doc.User?.Email ?? savedEmail ?? "doctor@cyhdoctorhub.com",
                        Role = doc.User?.Role ?? "Doctor",
                        Department = doc.Specialization,
                        DoctorProfileId = doc.Id,
                        AvatarUrl = doc.ProfilePic ?? doc.User?.AvatarUrl,
                        IsActive = doc.User?.IsActive ?? true
                    };

                    _authStateProvider.SetUser(restoredDocUser);
                    return restoredDocUser;
                }

                // If not found in DoctorProfiles, check if Admin role
                if (string.Equals(savedRole, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    var adminUser = new CurrentUserDTO
                    {
                        UserId = savedUserId,
                        FullName = "Hospital Chief Administrator",
                        Email = savedEmail ?? "admin@cyhdoctorhub.com",
                        Role = "Admin",
                        Department = "Hospital Operations",
                        IsActive = true
                    };
                    _authStateProvider.SetUser(adminUser);
                    return adminUser;
                }
            }

            // Second attempt: match by saved email if available
            if (!string.IsNullOrWhiteSpace(savedEmail))
            {
                var allDocs = await _doctorRepo.GetAllWithUsersAsync();
                var matched = allDocs.FirstOrDefault(d => string.Equals(d.User?.Email, savedEmail, StringComparison.OrdinalIgnoreCase));
                if (matched != null)
                {
                    var restoredDocUser = new CurrentUserDTO
                    {
                        UserId = matched.UserId,
                        FullName = matched.FullName,
                        Email = matched.User?.Email ?? savedEmail,
                        Role = matched.User?.Role ?? "Doctor",
                        Department = matched.Specialization,
                        DoctorProfileId = matched.Id,
                        AvatarUrl = matched.ProfilePic ?? matched.User?.AvatarUrl,
                        IsActive = matched.User?.IsActive ?? true
                    };

                    _authStateProvider.SetUser(restoredDocUser);
                    return restoredDocUser;
                }
            }
        }
        catch
        {
            // Interop not ready or storage restricted
        }

        return null;
    }
}
