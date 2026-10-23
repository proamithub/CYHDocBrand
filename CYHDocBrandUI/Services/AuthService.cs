using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
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
            CurrentUser.FullName == user.FullName;

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
            new("DoctorProfileId", user.DoctorProfileId?.ToString() ?? "")
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
    Task LogoutAsync();
    CurrentUserDTO? GetCurrentUser();
}

public class AuthService : IAuthService
{
    private readonly IDoctorRepository _doctorRepo;
    private readonly CustomAuthenticationStateProvider _authStateProvider;

    public AuthService(
        IDoctorRepository doctorRepo,
        CustomAuthenticationStateProvider authStateProvider)
    {
        _doctorRepo = doctorRepo;
        _authStateProvider = authStateProvider;
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return new LoginResult { Success = false, ErrorMessage = "Email and password are required." };

        // 1. Authenticate directly via SQL Stored Procedure (AuthenticateDoctor_CYH)
        var user = await _doctorRepo.AuthenticateAsync(email.Trim(), password);

        if (user == null)
        {
            return new LoginResult { Success = false, ErrorMessage = "Invalid credentials. Please verify your email and password." };
        }

        _authStateProvider.SetUser(user);

        return new LoginResult
        {
            Success = true,
            User = user
        };
    }

    public Task LogoutAsync()
    {
        _authStateProvider.MarkUserAsLoggedOut();
        return Task.CompletedTask;
    }

    public CurrentUserDTO? GetCurrentUser() => _authStateProvider.CurrentUser;
}
