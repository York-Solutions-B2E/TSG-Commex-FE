using TSG_Commex_FE.Components;
using TSG_Commex_FE.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MudBlazor.Services;
using Okta.AspNetCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        if (builder.Environment.IsDevelopment())
        {
            options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB
        }
    });

// Configure circuit options for better error messages in development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddServerSideBlazor(options =>
    {
        options.DetailedErrors = true;
    });
}

// Add MudBlazor services
builder.Services.AddMudServices();

// ✨ ADD BLAZOR SERVER AUTH SUPPORT
builder.Services.AddCascadingAuthenticationState();

// Add HttpContextAccessor for accessing the current HTTP context
builder.Services.AddHttpContextAccessor();

// Register the auth token handler
builder.Services.AddTransient<AuthTokenHandler>();

builder.Services.AddControllers();

// Add HttpClient for API calls with authentication
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]!);
})
.AddHttpMessageHandler<AuthTokenHandler>() // Add our auth handler
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();

    // Only bypass SSL validation in development
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }

    return handler;
});

// Configure application cookies
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOktaMvc(new OktaMvcOptions
{
    OktaDomain = builder.Configuration["Okta:OktaDomain"],
    ClientId = builder.Configuration["Okta:ClientId"],
    ClientSecret = builder.Configuration["Okta:ClientSecret"],
    AuthorizationServerId = "default",
    Scope = new List<string> { "openid", "profile", "email", "groups", "offline_access" },
    GetClaimsFromUserInfoEndpoint = true
});

// Configure OpenID Connect options to save tokens and handle roles
builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.SaveTokens = true; // Save tokens for API calls
    options.SignedOutCallbackPath = "/signout-callback-oidc";
    options.SignedOutRedirectUri = "/";
    
    // Map Okta groups to ASP.NET Core roles
    options.Events = new OpenIdConnectEvents()
    {
        OnTokenValidated = context =>
        {
            // Map groups from ID token to roles (REQUIRED for role-based authorization)
            var identity = context.Principal?.Identity as ClaimsIdentity;
            var groupsClaims = context.Principal?.Claims
                .Where(c => c.Type == "groups")
                .Select(c => c.Value)
                .ToList();
            
            foreach (var group in groupsClaims)
            {
                identity?.AddClaim(new Claim(ClaimTypes.Role, group));
            }

            return Task.CompletedTask;
        }
    };

});

// Add API service
// builder.Services.AddScoped<ApiService>();
// builder.Services.AddScoped<ICommunicationTypeService, CommunicationTypeService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
