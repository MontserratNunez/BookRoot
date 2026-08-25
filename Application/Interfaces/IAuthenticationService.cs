using Application.Common.Result;
using Application.Dtos.Auth;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IAuthenticationService
    {
        Task<Result<string>> SignUp(SignUpDto dto);
        Task<Result<string>> SignUpNoEmail(SignUpDto dto);
        Task<Result<TokenResponseDto>> Login(LoginDto dto);
        Task<Result<bool>> ResendConfirmationEmail(string email);
        Task<Result<bool>> SendPasswordResetEmail(string email);
        Task<Result<bool>> VerifyTokenHash(string tokenHash, string type);
        Task<Result<bool>> UpdatePassword(string newPassword);
        Task<Result<bool>> LogOut();
    }
}
