using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Book
{
    public class CreateInteractionDto
    {
        public string BookWorkKey { get; set; } = default!;
        public string Query {  get; set; } = default!;
        public InteractionStatus Status { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int? Rating { get; set; }
    }
}