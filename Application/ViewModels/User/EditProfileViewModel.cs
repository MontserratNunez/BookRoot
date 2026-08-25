using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.User
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(30, ErrorMessage = "El nombre de usuario no puede superar los 30 caracteres")]
        public string Username { get; set; } = default!;

        [StringLength(200, ErrorMessage = "La biografía no puede superar los 200 caracteres")]
        public string? Bio { get; set; }
    }
}
