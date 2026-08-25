namespace Domain.Interfaces
{
    public interface IAuthenticationRepository
    {
        //Temporary
        Task<(string? UserId, string? AccessToken)> SignUpNoEmail(string email, string password, string username, string? avatarUrl);


        Task<string?> SignUp(string email, string password, string username, string? avatarUrl);

        Task<(string? AccessToken, string? RefreshToken, string? UserId)> Login(string email, string password);

        Task<bool> DeleteAccount(string userId);

        Task<bool> ResendConfirmationEmail(string email);
        Task<bool> SendPasswordResetEmail(string email);
        Task<(bool IsSuccess, string? ErrorMessage)> VerifyTokenHash(string tokenHash, string type);
        Task<bool> UpdatePassword(string newPassword);
        Task<bool> LogOut();
    }
}