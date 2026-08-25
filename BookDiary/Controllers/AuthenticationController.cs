using Application.DTOs;
using Application.Interfaces;
using Application.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

namespace BookDiary.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationService _service;

        public AuthenticationController(IAuthenticationService service)
        {
            _service = service;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var actionName = context.ActionDescriptor.RouteValues["action"];

            if (User.Identity?.IsAuthenticated == true &&
                !string.Equals(actionName, nameof(LogOut), StringComparison.OrdinalIgnoreCase))
            {
                context.Result = RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new SignUpDto
            {
                Email = model.Email,
                Password = model.Password,
                Username = model.Username,
            };

            var result = await _service.SignUp(dto);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            

            return RedirectToAction("CheckEmail");
        }
        
        public IActionResult CheckEmail()
        {
            return View();
        }

        [HttpGet("/auth/confirm")]
        public async Task<IActionResult> ConfirmEmail(string token_hash, string type)
        {
            if (string.IsNullOrEmpty(token_hash) || string.IsNullOrEmpty(type))
                return RedirectToAction("Login");
                
            var result = await _service.VerifyTokenHash(token_hash, type);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToAction("Login");
            }
            TempData["SuccessMessage"] = "Correo confirmado exitosamente. Puedes iniciar sesión.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ResendConfirmation()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendConfirmation(Application.ViewModels.Auth.ResendConfirmationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
                
            var result = await _service.ResendConfirmationEmail(model.Email);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }
            
            TempData["SuccessMessage"] = "Correo reenviado. Revisa tu bandeja de entrada.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(Application.ViewModels.Auth.ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
                
            var result = await _service.SendPasswordResetEmail(model.Email);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }
            
            TempData["SuccessMessage"] = "Correo de recuperación enviado. Revisa tu bandeja de entrada.";
            return RedirectToAction("Login");
        }

        [HttpGet("/auth/reset-password")]
        public async Task<IActionResult> ResetPassword(string token_hash, string type)
        {
            if (string.IsNullOrEmpty(token_hash) || string.IsNullOrEmpty(type))
                return RedirectToAction("Login");
                
            var result = await _service.VerifyTokenHash(token_hash, type);
            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = "Token inválido o expirado.";
                return RedirectToAction("Login");
            }
            
            return View();
        }

        [HttpPost("/auth/reset-password")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(Application.ViewModels.Auth.ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
                
            var result = await _service.UpdatePassword(model.NewPassword);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }
            
            TempData["SuccessMessage"] = "Contraseña actualizada exitosamente.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var dto = new LoginDto
            {
                Email = model.Email,
                Password = model.Password
            };

            var result = await _service.Login(dto);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            Response.Cookies.Append("AuthToken", result.Data!.CustomJwt, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(2)
            });

            Response.Cookies.Append("SupabaseAccessToken", result.Data!.SupabaseAccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            Response.Cookies.Append("SupabaseRefreshToken", result.Data!.SupabaseRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            var result = await _service.LogOut();

            if (!result.IsSuccess)
            {
                TempData["AuthError"] = result.Message ?? "No se pudo cerrar la sesión. Inténtalo de nuevo.";
                return RedirectToAction("Index", "Home");
            }

            Response.Cookies.Delete("AuthToken");
            Response.Cookies.Delete("SupabaseAccessToken");
            Response.Cookies.Delete("SupabaseRefreshToken");
            return RedirectToAction(nameof(Login));
        }
    }
}