using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;

namespace BookDiary.Middlewares
{
    public class SupabaseRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SupabaseRefreshMiddleware> _logger;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public SupabaseRefreshMiddleware(RequestDelegate next, ILogger<SupabaseRefreshMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ISupabaseClientProvider clientProvider,
            IJwtTokenGenerator jwtGenerator,
            IUserRepository userRepository)
        {
            var supabaseAccessToken = context.Request.Cookies["SupabaseAccessToken"];
            var refreshToken = context.Request.Cookies["SupabaseRefreshToken"];

            // 1. Si no hay refresh token, el usuario no está autenticado
            if (string.IsNullOrEmpty(refreshToken))
            {
                await _next(context);
                return;
            }

            var client = clientProvider.GetClient();
            bool isSupabaseTokenExpired = string.IsNullOrEmpty(supabaseAccessToken) || IsTokenExpired(supabaseAccessToken);

            // 2. Vía rápida (Token válido): Asignar header en memoria a través de Options
            if (!isSupabaseTokenExpired)
            {
                client.Postgrest.Options.Headers["Authorization"] = $"Bearer {supabaseAccessToken}";
                await _next(context);
                return;
            }

            // 3. Token expirado: Refresco controlado con semáforo
            await _semaphore.WaitAsync();
            try
            {
                // Re-verificar por si otra petición concurrente ya renovó el token
                var currentAccessToken = context.Request.Cookies["SupabaseAccessToken"];
                if (!string.IsNullOrEmpty(currentAccessToken) && !IsTokenExpired(currentAccessToken))
                {
                    client.Postgrest.Options.Headers["Authorization"] = $"Bearer {currentAccessToken}";
                    await _next(context);
                    return;
                }

                var supabaseSession = await client.Auth.SetSession(
                    supabaseAccessToken ?? string.Empty,
                    refreshToken,
                    forceAccessTokenRefresh: true
                );

                if (supabaseSession?.User != null && !string.IsNullOrEmpty(supabaseSession.AccessToken))
                {
                    // Sincronizar el token renovado en Postgrest.Options antes de consultar DB
                    client.Postgrest.Options.Headers["Authorization"] = $"Bearer {supabaseSession.AccessToken}";

                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddDays(7)
                    };

                    context.Response.Cookies.Append("SupabaseAccessToken", supabaseSession.AccessToken, cookieOptions);

                    if (!string.IsNullOrEmpty(supabaseSession.RefreshToken))
                    {
                        context.Response.Cookies.Append("SupabaseRefreshToken", supabaseSession.RefreshToken, cookieOptions);
                    }

                    var profile = await userRepository.GetProfileById(supabaseSession.User.Id);
                    var newCustomJwt = jwtGenerator.Generate(supabaseSession.User.Id, supabaseSession.User.Email ?? "", profile?.Username ?? "");

                    context.Response.Cookies.Append("AuthToken", newCustomJwt, cookieOptions);
                    context.Request.Headers["Authorization"] = $"Bearer {newCustomJwt}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falló el refresco de sesión en Supabase — Limpiando cookies.");
                context.Response.Cookies.Delete("AuthToken");
                context.Response.Cookies.Delete("SupabaseAccessToken");
                context.Response.Cookies.Delete("SupabaseRefreshToken");
            }
            finally
            {
                _semaphore.Release();
            }

            await _next(context);
        }

        private static bool IsTokenExpired(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                return jwtToken.ValidTo <= DateTime.UtcNow.AddSeconds(15);
            }
            catch
            {
                return true;
            }
        }
    }
}