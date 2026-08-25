using Application.Common.Result;
using Application.Dtos.Auth;
using Application.DTOs;
using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAuthenticationRepository _repo;
        private readonly IJwtTokenGenerator _jwt;
        private readonly IUserRepository _userRepository;

        public AuthenticationService(IAuthenticationRepository repo, IJwtTokenGenerator jwt, IUserRepository userRepository)
        {
            _repo = repo;
            _jwt = jwt;
            _userRepository = userRepository;
        }

        public async Task<Result<string>> SignUpNoEmail(SignUpDto dto)
        {
            var result = new Result<string>();

            if (string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password) ||
                string.IsNullOrWhiteSpace(dto.Username))
            {
                result.IsSuccess = false;
                result.Message = "Datos inválidos";
                return result;
            }

            try {
                var (userId, accessToken) = await _repo.SignUpNoEmail(dto.Email, dto.Password, dto.Username, "");

            
                if (userId == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Error creando usuario";
                    return result;
                }
            

                result.IsSuccess = true;
                result.Message = "Usuario creado correctamente. Confirme su email.";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Error al cargar el Inicio";
            }
            return result;
        }


        //code for when i implement smtp
        public async Task<Result<string>> SignUp(SignUpDto dto)
        {
            var result = new Result<string>();

            if (string.IsNullOrWhiteSpace(dto.Email))
                result.Errors.Add("Email requerido");

            if (string.IsNullOrWhiteSpace(dto.Password))
                result.Errors.Add("Password requerido");

            if (string.IsNullOrWhiteSpace(dto.Username))
                result.Errors.Add("Username requerido");

            if (result.Errors.Any())
            {
                result.IsSuccess = false;
                result.Message = "Errores de validación";
                return result;
            }

            //SignUpEmail to avoid exception
            var userId = await _repo.SignUp(
                dto.Email,
                dto.Password,
                dto.Username,
                ""
            );

            if (userId == null)
            {
                result.IsSuccess = false;
                result.Message = "Error creando usuario";
                return result;
            }

            result.IsSuccess = true;
            result.Message = "Usuario creado correctamente. Revisa tu correo para confirmar.";

            return result;
        }

        public async Task<Result<TokenResponseDto>> Login(LoginDto dto)
        {
            var result = new Result<TokenResponseDto>();

            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            {
                result.IsSuccess = false;
                result.Message = "Credenciales inválidas";
                return result;
            }
            
            try
            {
                var (accessToken, refreshToken, userId) = await _repo.Login(dto.Email, dto.Password);

                if (userId == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Credenciales inválidas o correo no confirmado";
                    return result;
                }

                var profile = await _userRepository.GetProfileById(userId);
                var username = profile?.Username;

                var jwt = _jwt.Generate(userId, dto.Email, username);

                result.IsSuccess = true;
                result.Data = new TokenResponseDto
                {
                    CustomJwt = jwt,
                    SupabaseAccessToken = accessToken,
                    SupabaseRefreshToken = refreshToken
                };
            
               
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Error al iniciar sesión, intentelo de nuevo";
            }
            
            return result;
        }

        public async Task<Result<bool>> ResendConfirmationEmail(string email)
        {
            var result = new Result<bool>();
            if (string.IsNullOrWhiteSpace(email))
            {
                result.IsSuccess = false;
                result.Message = "El email es requerido.";
                return result;
            }

            result.IsSuccess = await _repo.ResendConfirmationEmail(email);
            result.Message = result.IsSuccess ? "Correo reenviado exitosamente." : "No se pudo reenviar el correo, intente más tarde.";
            return result;
        }

        public async Task<Result<bool>> SendPasswordResetEmail(string email)
        {
            var result = new Result<bool>();
            if (string.IsNullOrWhiteSpace(email))
            {
                result.IsSuccess = false;
                result.Message = "El email es requerido.";
                return result;
            }

            result.IsSuccess = await _repo.SendPasswordResetEmail(email);
            result.Message = result.IsSuccess ? "Correo de recuperación enviado exitosamente." : "No se pudo enviar el correo, intente más tarde.";
            return result;
        }

        public async Task<Result<bool>> VerifyTokenHash(string tokenHash, string type)
        {
            var result = new Result<bool>();
            if (string.IsNullOrWhiteSpace(tokenHash) || string.IsNullOrWhiteSpace(type))
            {
                result.IsSuccess = false;
                result.Message = "Token inválido.";
                return result;
            }

            var res = await _repo.VerifyTokenHash(tokenHash, type);
            result.IsSuccess = res.IsSuccess;
            result.Message = res.IsSuccess ? "Token verificado exitosamente." : res.ErrorMessage ?? "Error al verificar el token.";
            return result;
        }

        public async Task<Result<bool>> UpdatePassword(string newPassword)
        {
            var result = new Result<bool>();
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                result.IsSuccess = false;
                result.Message = "La contraseña debe tener al menos 6 caracteres.";
                return result;
            }

            result.IsSuccess = await _repo.UpdatePassword(newPassword);
            result.Message = result.IsSuccess ? "Contraseña actualizada exitosamente." : "Error al actualizar la contraseña.";
            return result;
        }

        public async Task<Result<bool>> LogOut()
        {
            var result = new Result<bool>();
            var success = await _repo.LogOut();

            if (!success)
            {
                result.IsSuccess = false;
                result.Message = "Error al comunicar con el servidor de autenticación.";
                result.Data = false;
                return result;
            }

            result.IsSuccess = true;
            result.Data = true;
            return result;
        }
    }

}