using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Book
{
    public class EditReadedBookDto
    {
        public string Id { get; set; }
        public DateTime? Date { get; set; }

        public int? Rating { get; set; }
    }
}
