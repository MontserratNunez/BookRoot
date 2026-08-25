using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Book
{
    public class UserListsViewModel
    {
        public List<ReadingViewModel> Reading { get; set; } = new();
        public List<CompletedViewModel> Completed { get; set; } = new();
    }
}
