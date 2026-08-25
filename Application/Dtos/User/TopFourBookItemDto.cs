using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.User
{
    public class TopFourBookItemDto
    {
        public string BookWorkKey { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string? CoverUrl { get; set; }
    }
}
