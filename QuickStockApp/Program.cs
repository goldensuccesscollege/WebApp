using QuickStockApp.Services;

var builder = WebApplication.CreateBuilder(args);

// SSL Bypass for Local Development
var handler = new HttpClientHandler();
if (builder.Environment.IsDevelopment())
{
    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
}

// Add services
builder.Services.AddRazorPages();

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    options.HeaderName = "RequestVerificationToken";
});

// Register ApiService
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    var apiUrl = builder.Configuration["ApiSettings:BaseUrl"];
    if (string.IsNullOrWhiteSpace(apiUrl))
        throw new Exception("ApiSettings:BaseUrl missing in appsettings.json");

    client.BaseAddress = new Uri(apiUrl);
}).ConfigurePrimaryHttpMessageHandler(() => handler);

// ✅ Authentication & Authorization
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

        // ✅ Ensure cookie is sent only over HTTPS
        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    // ✅ Apply HTTPS redirection only in Production so local HTTP works
    app.UseHttpsRedirection();
}

// ✅ ORDER MATTERS
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();   // FIRST
app.UseAuthorization();    // SECOND

app.MapRazorPages();

app.Run();
