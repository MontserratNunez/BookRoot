using System.ComponentModel.DataAnnotations;

namespace Application.ViewModels.Lists
{
    public class CreateListViewModel
    {
        [Required(ErrorMessage = "El nombre de la lista es obligatorio.")]
        [StringLength(80, ErrorMessage = "El nombre no puede superar los 80 caracteres.")]
        public string ListName { get; set; } = default!;

        [StringLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres.")]
        public string? ListDescription { get; set; }
    }
}
