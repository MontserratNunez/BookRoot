using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Journal
{
    public class JournalViewModel
    {
        public string Username { get; set; } = default!;
        public List<JournalYearGroupViewModel> Years { get; set; } = new();
    }

    public class JournalYearGroupViewModel
    {
        public int Year { get; set; }
        public List<JournalMonthGroupViewModel> Months { get; set; } = new();
    }

    public class JournalMonthGroupViewModel
    {
        public string MonthName { get; set; } = default!;
        public int MonthNumber { get; set; }
        public List<JournalBookViewModel> Books { get; set; } = new();
    }

    public class JournalBookViewModel
    {
        public string BookWorkKey { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Author { get; set; } = default!;
        public string? CoverUrl { get; set; }
        public int? Rating { get; set; }
        public string FinishedDay { get; set; } = default!;
    }
}
