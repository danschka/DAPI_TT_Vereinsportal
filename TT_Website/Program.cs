using TT_Website.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using TT_Website.Data;
using TT_Website.Services;

namespace TT_Website
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<GalleryService>();
            builder.Services.AddScoped<DownloadService>();
            builder.Services.AddScoped<SponsorService>();
            builder.Services.AddScoped<MemberApplicationService>();
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddScoped<NewsService>();
            builder.Services.AddHttpClient<MyTischtennisImportService>();
            builder.Services.AddHttpClient<MyTischtennisNewsService>();
            builder.Services.AddScoped<TeamService>();
            builder.Services.AddScoped<FileUploadService>();
            builder.Services.AddScoped<ContentPageService>();
            builder.Services.AddScoped<SiteSettingsService>();
            builder.Services.AddScoped<SiteSeedService>();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "TT_Website.Admin";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.ExpireTimeSpan = TimeSpan.FromHours(2);
                    options.LoginPath = "/admin/login";
                    options.AccessDeniedPath = "/admin/login";
                    options.SlidingExpiration = true;
                });

            builder.Services.AddAuthorization();
            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy("admin-login", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));
            });

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                dbContext.Database.Migrate();

                var seedService = scope.ServiceProvider.GetRequiredService<SiteSeedService>();
                seedService.SeedAsync(
                    scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>(),
                    scope.ServiceProvider.GetRequiredService<IConfiguration>()).GetAwaiter().GetResult();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();

            app.UseAntiforgery();

            app.Use(async (context, next) =>
            {
                var path = context.Request.Path;

                if (path.StartsWithSegments("/admin") &&
                    !path.StartsWithSegments("/admin/login") &&
                    !path.StartsWithSegments("/admin/auth/login") &&
                    context.User.Identity?.IsAuthenticated != true)
                {
                    var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
                    context.Response.Redirect($"/admin/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                    return;
                }

                await next();
            });

            app.MapPost("/admin/auth/login", async (
                HttpContext context,
                IConfiguration configuration,
                IAntiforgery antiforgery) =>
            {
                await antiforgery.ValidateRequestAsync(context);

                var form = await context.Request.ReadFormAsync();
                var password = form["password"].ToString();
                var returnUrl = GetSafeAdminReturnUrl(form["returnUrl"].ToString());
                var adminPassword = configuration["AdminSettings:Password"];

                if (string.IsNullOrWhiteSpace(adminPassword) ||
                    !PasswordsMatch(password, adminPassword))
                {
                    return Results.Redirect($"/admin/login?error=1&returnUrl={Uri.EscapeDataString(returnUrl)}");
                }

                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, "Admin"),
                    new(ClaimTypes.Role, "Admin")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await context.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        AllowRefresh = true,
                        IssuedUtc = DateTimeOffset.UtcNow
                    });

                return Results.Redirect(returnUrl);
            }).RequireRateLimiting("admin-login");

            app.MapPost("/admin/auth/logout", async (
                HttpContext context,
                IAntiforgery antiforgery) =>
            {
                await antiforgery.ValidateRequestAsync(context);
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.Redirect("/admin/login?loggedOut=1");
            }).RequireAuthorization();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }

        private static bool PasswordsMatch(string submittedPassword, string configuredPassword)
        {
            var submittedHash = SHA256.HashData(Encoding.UTF8.GetBytes(submittedPassword));
            var configuredHash = SHA256.HashData(Encoding.UTF8.GetBytes(configuredPassword));

            return CryptographicOperations.FixedTimeEquals(submittedHash, configuredHash);
        }

        private static string GetSafeAdminReturnUrl(string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl) ||
                !returnUrl.StartsWith("/", StringComparison.Ordinal) ||
                returnUrl.StartsWith("//", StringComparison.Ordinal) ||
                !returnUrl.StartsWith("/admin", StringComparison.OrdinalIgnoreCase) ||
                returnUrl.StartsWith("/admin/auth", StringComparison.OrdinalIgnoreCase) ||
                returnUrl.StartsWith("/admin/login", StringComparison.OrdinalIgnoreCase))
            {
                return "/admin";
            }

            return returnUrl;
        }
    }
}
