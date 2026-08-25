using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels
{
    public class BookDetailsViewModel
    {
        public string Title { get; set; } = default!;
        public string? Author { get; set; }
        public int? Year { get; set; }

        public string? BookWorkKey { get; set; }
        public string? CoverEditionKey { get; set; }

        public double? AverageRating { get; set; }

        public bool IsInReading { get; set; }

        

        public bool IsCompleted { get; set; }

        public DateTime? SelfFinished { get; set; }
        public int? SelfRating { get; set; }
        public string IntId { get; set; }
        public bool IsFavorite { get; set; }
        public int FavoriteSlotIndex { get; set; } = -1;




    }
}
