using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Home
{
    public class HomeBookViewModel
    {
        public string BookWorkKey { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Author { get; set; } = default!;
        public string? CoverUrl { get; set; }
        public string DisplayRating { get; set; } = default!;
    }
}
