using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Book
{
    public class UserListsDto
    {
        public List<ReadingDto> Reading { get; set; } = new();
        public List<CompletedDto> Completed { get; set; } = new();
    }
}
