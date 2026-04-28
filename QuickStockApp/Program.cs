using QuickStockApp.Services;
using QuickStockApp.Filters;

var builder = WebApplication.CreateBuilder(args);

// SSL Bypass logic moved inside ConfigurePrimaryHttpMessageHandler

// Register services
builder.Services.AddRazorPages().AddMvcOptions(options =>
{
    options.Filters.Add<CampusSelectionFilter>();
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<UnauthorizedHandler>();

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
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
})
.ConfigurePrimaryHttpMessageHandler(() => 
{
    var newHandler = new HttpClientHandler();
    if (builder.Environment.IsDevelopment())
    {
        newHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
    }
    return newHandler;
})
.AddHttpMessageHandler<UnauthorizedHandler>();

// ✅ Authentication & Authorization
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);

        // ✅ Ensure cookie is sent only over HTTPS
        options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// ✅ ORDER MATTERS
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();   // FIRST
app.UseAuthorization();    // SECOND

app.MapRazorPages();

app.MapGet("/", () => Results.Redirect("/Account/Login"));

app.Run();
