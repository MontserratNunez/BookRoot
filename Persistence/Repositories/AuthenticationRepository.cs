using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Supabase.Interfaces;
using System.Text;

public class AuthenticationRepository : IAuthenticationRepository
{
    private readonly ISupabaseClientProvider _provider;
    private readonly IConfiguration _config;

    public AuthenticationRepository(ISupabaseClientProvider provider, IConfiguration config)
    {
        _provider = provider;
        _config = config;
    }


    //email confirm temp disabled
    public async Task<(string? UserId, string? AccessToken)> SignUpNoEmail(string email, string password, string username, string? avatarUrl)
    {
        var client = _provider.GetClient();

        try
        {
            var response = await client.Auth.SignUp(email, password,
                new Supabase.Gotrue.SignUpOptions
                {
                    Data = new Dictionary<string, object>
                    {
                        { "username", username },
                        { "avatar_url", "" }
                    }
                });

            if (response == null || response.User == null)
                return (null, null);

            return (
                response.User.Id,
                response.AccessToken
            );
        }
        catch (Exception)
        {
            return (null, null);
        }
    }

    //code for when i implement smtp
    public async Task<string?> SignUp(string email, string password, string username, string? avatarUrl)
    {
        var client = _provider.GetClient();

        var response = await client.Auth.SignUp(email, password,
            new Supabase.Gotrue.SignUpOptions
            {
                Data = new Dictionary<string, object>
                {
                    { "username", username },
                    { "avatar_url", "" }
                }
            });

        return response?.User?.Id;
    }

    public async Task<(string? AccessToken, string? RefreshToken, string? UserId)> Login(string email, string password)
    {
        var client = _provider.GetClient();

        var response = await client.Auth.SignIn(email, password);


        if (response == null || response.User == null)
            return (null, null, null);

        return (
            response.AccessToken,
            response.RefreshToken,
            response.User.Id
        );
    }

    public async Task<bool> DeleteAccount(string userId)
    {
        var supabaseUrl = _config["SUPABASE_URL"];
        var serviceKey = _config["SUPABASE_SERVICE_KEY"];

        if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(serviceKey))
            return false;

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("apikey", serviceKey);
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {serviceKey}");

        var url = $"{supabaseUrl}/auth/v1/admin/users/{userId}";
        var response = await httpClient.DeleteAsync(url);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ResendConfirmationEmail(string email)
    {
        try
        {
            using var httpClient = new HttpClient();
            var supabaseUrl = _config["SUPABASE_URL"];
            var supabaseKey = _config["SUPABASE_KEY"];

            if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
                return false;

            httpClient.DefaultRequestHeaders.Add("apikey", supabaseKey);

            var body = new
            {
                type = "signup",
                email = email
            };

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            var response = await httpClient.PostAsync($"{supabaseUrl}/auth/v1/resend", content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> SendPasswordResetEmail(string email)
    {
        var client = _provider.GetClient();
        try
        {
            var webUrl = _config["WEB_PAGE_URL"] ?? "https://localhost:7287";
            var redirectUrl = $"{webUrl}/auth/reset-password";
            
            var res = await client.Auth.ResetPasswordForEmail(email);
            return res;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> VerifyTokenHash(string tokenHash, string type)
    {
        try
        {
            using var httpClient = new System.Net.Http.HttpClient();
            var supabaseUrl = _config["SUPABASE_URL"];
            var supabaseKey = _config["SUPABASE_KEY"];

            httpClient.DefaultRequestHeaders.Add("apikey", supabaseKey);
            
            var body = new { token_hash = tokenHash, type = type.ToLower() };
            var content = new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(body), System.Text.Encoding.UTF8, "application/json");
            
            var response = await httpClient.PostAsync($"{supabaseUrl}/auth/v1/verify", content);
            
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            return (false, "Error verifying token.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<bool> UpdatePassword(string newPassword)
    {
        var client = _provider.GetClient();
        try
        {
            var user = await client.Auth.Update(new Supabase.Gotrue.UserAttributes { Password = newPassword });
            return user != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> LogOut()
    {
        var client = _provider.GetClient();
        try
        {
            await client.Auth.SignOut();
        }
        catch
        {
            return false;
        }

        return true;
    }
}