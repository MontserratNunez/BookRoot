using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Book
{
    public class BookInfoDto
    {
        public string Title { get; set; }
        public string? Author { get; set; }

        public required string? BookWorkKey { get; set; }
        public required string? CoverEditionKey { get; set; }

        public int? FirstPublishYear { get; set; }
    }
}
