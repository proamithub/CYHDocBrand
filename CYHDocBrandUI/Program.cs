using Microsoft.AspNetCore.Components.Authorization;
using CYHDocBrandData;
using CYHDocBrandRepo.IRepository;
using CYHDocBrandRepo.Repository;
using CYHDocBrandSMS;
using CYHDocBrandUI.Components;
using CYHDocBrandUI.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Connection String (SQL Server Stored Procedures)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=VRS-AMITPC\\SQLEXPRESS;Database=CYHDocBrandBD;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

// 2. Authentication & Authorization (Custom Stored-Procedure Auth State)
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => 
    sp.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddScoped<IAuthService, AuthService>();

// 3. Stored Procedure Repositories (Pure ADO.NET)
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();

// 4. Application & Notification Services
builder.Services.AddScoped<ISMSService, SMSService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// 5. Razor Components (Interactive Server)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// 6. Initialize Database via SQL Server Stored Procedure
try
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    await DbInitializer.InitializeAsync(connectionString, logger);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while initializing via Stored Procedure.");
}

// 7. Pipeline Configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
