using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Book
{
    public class CreateInteractionViewModel
    {
        [Required]
        public string BookWorkKey { get; set; } = default!;
        public string Query { get; set; } = default!;
        [Required]
        public InteractionStatus Status { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int? Rating { get; set; }
    }
}
