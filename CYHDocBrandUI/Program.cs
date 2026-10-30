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
    ?? "Server=AMITLAP;Database=CYHDocBrandBD;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Pooling=true;Min Pool Size=5;Max Pool Size=100;Connect Timeout=15;";

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
builder.Services.AddScoped<IMediaPlatformRepository, MediaPlatformRepository>();
builder.Services.AddScoped<IPushNotificationRepository, PushNotificationRepository>();

// 4. Application & Notification Services
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<ISMSService, SMSService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<EmailVerificationTokenService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IBannerAiService, BannerAiService>();
builder.Services.AddScoped<IWebPushNotificationService, WebPushNotificationService>();
builder.Services.AddHostedService<BannerPublishBackgroundWorker>();

// 5. Performance Optimizations (Compression & SignalR Speed)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "image/svg+xml",
        "application/wasm",
        "text/json",
        "application/json",
        "application/manifest+json"
    });
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10 MB for High-DPI Canvas transfers
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(45);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
});

// 6. Razor Components (Interactive Server)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(30);
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(2);
        options.DisconnectedCircuitMaxRetained = 50;
        options.MaxBufferedUnacknowledgedRenderBatches = 10;
    });

var app = builder.Build();

// 7. Initialize Database via SQL Server Stored Procedure
try
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    await DbInitializer.InitializeAsync(connectionString, logger);
    await MediaPlatformInitializer.InitializeAsync(connectionString, logger);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while initializing via Stored Procedure.");
}

if (args.Contains("--seed-only"))
{
    Console.WriteLine("Database initialization and seed complete!");
    return;
}



// 8. Pipeline Configuration
app.UseResponseCompression();

// Initialize Asset Versioning engine for automatic cache busting
AssetVersioning.Initialize(app.Environment.ContentRootPath);

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
        // Smart Cache-Control: Fingerprinted assets cached 1 year; unversioned CSS/JS always revalidated
        var path = ctx.Context.Request.Path.Value ?? "";
        var hasVersionQuery = ctx.Context.Request.Query.ContainsKey("v");

        if (hasVersionQuery)
        {
            // Fingerprinted/versioned assets (?v=...): immutable cache for 1 year
            ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=31536000,immutable";
        }
        else if (path.EndsWith("service-worker.js", StringComparison.OrdinalIgnoreCase))
        {
            // Service worker script MUST never be cached by browser
            ctx.Context.Response.Headers["Cache-Control"] = "no-cache,no-store,must-revalidate";
            ctx.Context.Response.Headers["Pragma"] = "no-cache";
            ctx.Context.Response.Headers["Expires"] = "0";
        }
        else if (path.EndsWith(".css", StringComparison.OrdinalIgnoreCase) || 
                 path.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
        {
            // Unversioned CSS and JS must revalidate on each request so live code refreshes immediately
            ctx.Context.Response.Headers["Cache-Control"] = "no-cache,must-revalidate";
            ctx.Context.Response.Headers["Pragma"] = "no-cache";
            ctx.Context.Response.Headers["Expires"] = "0";
        }
        else
        {
            // Standard static assets (images, fonts): 7 days browser cache
            ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=604800";
        }
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

// 10. Email Verification Gateway Endpoint
app.MapGet("/api/verify-email", async (
    string? token,
    EmailVerificationTokenService tokenService,
    IDoctorRepository doctorRepo) =>
{
    var validation = tokenService.ValidateToken(token);
    if (!validation.IsValid || string.IsNullOrEmpty(validation.UserId))
    {
        var errorMsg = Uri.EscapeDataString(validation.ErrorMessage ?? "Invalid or expired verification link.");
        return Results.Redirect($"/verify-email?status=error&message={errorMsg}");
    }

    var success = await doctorRepo.ConfirmDoctorEmailAsync(validation.UserId);
    if (success)
    {
        var doc = await doctorRepo.GetByUserIdAsync(validation.UserId);
        var nameParam = doc != null && !string.IsNullOrWhiteSpace(doc.FullName) 
            ? $"&name={Uri.EscapeDataString(doc.FullName)}" 
            : "";
        var emailParam = validation.Email != null 
            ? $"&email={Uri.EscapeDataString(validation.Email)}" 
            : "";
        return Results.Redirect($"/verify-email?status=success{nameParam}{emailParam}");
    }
    else
    {
        return Results.Redirect("/verify-email?status=error&message=" + Uri.EscapeDataString("Failed to confirm email in database. Please try again."));
    }
});

app.Run();
