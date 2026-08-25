using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Home
{
    public class MostReadBookDto
    {
        public string BookWorkKey { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string? CoverUrl { get; set; }
        public double? AverageRating { get; set; }
        public int ReadCount { get; set; }
    }
}
