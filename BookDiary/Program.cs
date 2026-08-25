using Application;
using Application.Interfaces;
using BookDiary.Middlewares;
using Infraestructure;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using System.Net;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient<IOpenLibraryService, OpenLibraryService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddPersistenceLayerIoc(builder.Configuration);
builder.Services.AddApplicationLayerIoc();
builder.Services.AddInfraestructureLayerIoc();

builder.Services.AddControllersWithViews();

builder.Services.AddMemoryCache();

builder.Services.AddAuthorization();

var bytes = Encoding.UTF8.GetBytes(builder.Configuration["Authentication:JwtSecret"]!);

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(bytes),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Authentication:ValidAudience"],
            ValidIssuer = builder.Configuration["Authentication:ValidIssuer"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.Name
        };

        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Headers.TryGetValue("Authorization", out var authHeader) &&
                    authHeader.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authHeader.ToString()["Bearer ".Length..].Trim();
                }
                else if (context.Request.Cookies.TryGetValue("AuthToken", out var cookieToken))
                {
                    context.Token = cookieToken;
                }

                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();

                if (!context.Response.HasStarted)
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    }
                    else
                    {
                        context.Response.Redirect("/");
                    }
                }

                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

app.UseExceptionHandler("/Error/500");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// ── Security Headers (OWASP A05 – Security Misconfiguration) ─────────────────
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    var isDev   = app.Environment.IsDevelopment();

    // Prevent clickjacking
    headers["X-Frame-Options"] = "DENY";

    // Prevent MIME-type sniffing
    headers["X-Content-Type-Options"] = "nosniff";

    // Control referrer information sent to external sites
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    // Restrict browser features
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

    // ── Content Security Policy ───────────────────────────────────────────────
    // OpenLibrary serves covers from two hostnames:
    //   - https://covers.openlibrary.org  (primary)
    //   - https://archive.org             (fallback, same CDN)
    // ─────────────────────────────────────────────────────────────────────────
    
    // Generate a secure nonce for this request
    var nonceBytes = new byte[32];
    System.Security.Cryptography.RandomNumberGenerator.Fill(nonceBytes);
    var nonce = Convert.ToBase64String(nonceBytes);
    context.Items["csp-nonce"] = nonce;

    var scriptSrc = isDev
        ? $"'self' 'unsafe-inline' 'nonce-{nonce}' https://cdn.jsdelivr.net https://code.jquery.com"
        : $"'self' 'nonce-{nonce}' https://cdn.jsdelivr.net https://code.jquery.com";

    var connectSrc = isDev
        ? "'self' ws://localhost:* wss://localhost:* http://localhost:* https://localhost:*"
        : "'self'";

    headers["Content-Security-Policy"] =
        $"default-src 'self'; " +
        $"script-src {scriptSrc}; " +
        $"style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://fonts.googleapis.com; " +
        $"font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net; " +
        $"img-src 'self' data: https://covers.openlibrary.org https://archive.org https://*.archive.org https://cdn-icons-png.flaticon.com; " +
        $"connect-src {connectSrc}; " +
        $"frame-ancestors 'none'; " +
        $"form-action 'self';";

    await next();
});
// ─────────────────────────────────────────────────────────────────────────────

app.UseStaticFiles();

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseRouting();

app.UseMiddleware<SupabaseRefreshMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Public}/{action=Index}/{id?}");

app.Run();
