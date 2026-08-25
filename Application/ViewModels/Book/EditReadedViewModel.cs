using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Book
{
    public class EditReadedViewModel
    {
        public string Id { get; set; }

        public DateTime? Date { get; set; }

        [Range(0, 5)]
        public int? Rating { get; set; }
    }
}
