using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels
{
    public class SignUpViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = default!;

        [Required]
        public string Username { get; set; } = default!;
    }
}