using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Book
{
    public class CompletedDto
    {
        public string Id { get; set; }
        public string BookWorkKey { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Author { get; set; } = default!;
        public int? Rating { get; set; }
        public DateTime? FinishedAt { get; set; }
    }
}
