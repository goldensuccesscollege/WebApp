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

// Helper to configure HttpClients
Action<HttpClient> configureClient = client =>
{
    var apiUrl = builder.Configuration["ApiSettings:BaseUrl"];
    if (string.IsNullOrWhiteSpace(apiUrl))
        throw new Exception("ApiSettings:BaseUrl missing in appsettings.json");
    client.BaseAddress = new Uri(apiUrl);
};

// Register ApiServices by Feature
builder.Services.AddHttpClient<IAuthService, AuthService>(configureClient)
    .ConfigurePrimaryHttpMessageHandler(() => GetHandler(builder.Environment.IsDevelopment()))
    .AddHttpMessageHandler<UnauthorizedHandler>();

builder.Services.AddHttpClient<IAssetService, AssetService>(configureClient)
    .ConfigurePrimaryHttpMessageHandler(() => GetHandler(builder.Environment.IsDevelopment()))
    .AddHttpMessageHandler<UnauthorizedHandler>();

builder.Services.AddHttpClient<IRoomService, RoomService>(configureClient)
    .ConfigurePrimaryHttpMessageHandler(() => GetHandler(builder.Environment.IsDevelopment()))
    .AddHttpMessageHandler<UnauthorizedHandler>();

builder.Services.AddHttpClient<ICampusService, CampusService>(configureClient)
    .ConfigurePrimaryHttpMessageHandler(() => GetHandler(builder.Environment.IsDevelopment()))
    .AddHttpMessageHandler<UnauthorizedHandler>();

builder.Services.AddHttpClient<IReportService, ReportService>(configureClient)
    .ConfigurePrimaryHttpMessageHandler(() => GetHandler(builder.Environment.IsDevelopment()))
    .AddHttpMessageHandler<UnauthorizedHandler>();

builder.Services.AddHttpClient<IApparelService, ApparelService>(configureClient)
    .ConfigurePrimaryHttpMessageHandler(() => GetHandler(builder.Environment.IsDevelopment()))
    .AddHttpMessageHandler<UnauthorizedHandler>();

builder.Services.AddHttpClient<IFurnitureService, FurnitureService>(configureClient)
    .ConfigurePrimaryHttpMessageHandler(() => GetHandler(builder.Environment.IsDevelopment()))
    .AddHttpMessageHandler<UnauthorizedHandler>();

builder.Services.AddHttpClient<ILibraryService, LibraryService>(configureClient)
    .ConfigurePrimaryHttpMessageHandler(() => GetHandler(builder.Environment.IsDevelopment()))
    .AddHttpMessageHandler<UnauthorizedHandler>();

builder.Services.AddHttpClient<IConsumableService, ConsumableService>(configureClient)
    .ConfigurePrimaryHttpMessageHandler(() => GetHandler(builder.Environment.IsDevelopment()))
    .AddHttpMessageHandler<UnauthorizedHandler>();

builder.Services.AddHttpClient<IProfileService, ProfileService>(configureClient)
    .ConfigurePrimaryHttpMessageHandler(() => GetHandler(builder.Environment.IsDevelopment()))
    .AddHttpMessageHandler<UnauthorizedHandler>();

builder.Services.AddHttpClient<IUserManagementService, UserManagementService>(configureClient)
    .ConfigurePrimaryHttpMessageHandler(() => GetHandler(builder.Environment.IsDevelopment()))
    .AddHttpMessageHandler<UnauthorizedHandler>();

// Keep IApiService for backward compatibility (delegating to specialized services)
builder.Services.AddTransient<IApiService, ApiService>();

static HttpClientHandler GetHandler(bool isDev)
{
    var handler = new HttpClientHandler();
    if (isDev) handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
    return handler;
}

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
