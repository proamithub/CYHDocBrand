using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
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
builder.Services.AddScoped<IBannerRepository, BannerRepository>();
builder.Services.AddScoped<IBannerTemplateRepository, BannerTemplateRepository>();
builder.Services.AddScoped<IDoctorAiSettingRepository, DoctorAiSettingRepository>();
builder.Services.AddScoped<ICreditRepository, CreditRepository>();
builder.Services.AddScoped<ISocialAccountRepository, SocialAccountRepository>();
builder.Services.AddScoped<IBannerScheduleRepository, BannerScheduleRepository>();
builder.Services.AddScoped<IPushNotificationRepository, PushNotificationRepository>();

// 4. Application & Notification Services
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ISMSService, SMSService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IBannerAiService, BannerAiService>();
builder.Services.AddSingleton<IWebPushNotificationService, WebPushNotificationService>();
builder.Services.AddHostedService<BannerPublishBackgroundWorker>();

// 5. Performance Optimizations (Compression & SignalR Speed)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB for High-DPI Canvas transfers
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

// 6. Razor Components (Interactive Server)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(30);
    });

var app = builder.Build();

// 7. Initialize Database via SQL Server Stored Procedure
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

// 8. Pipeline Configuration
app.UseResponseCompression();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // 7 days browser cache for images, fonts, scripts
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=604800");
    }
});

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// 9. Web Push Notification Endpoints
app.MapGet("/api/push/public-key", (IWebPushNotificationService pushService) => 
    Results.Ok(new { publicKey = pushService.PublicKey }));

app.MapPost("/api/push/subscribe", async (CYHDocBrandModel.DTO.PushSubscriptionRequest request, IPushNotificationRepository pushRepo) =>
{
    if (request == null || string.IsNullOrWhiteSpace(request.Endpoint))
        return Results.BadRequest(new { success = false, message = "Invalid subscription" });

    var ok = await pushRepo.SaveSubscriptionAsync(request);
    return Results.Ok(new { success = ok });
});

app.Run();
