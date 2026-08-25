using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.User
{
    public class UserSearchDto
    {
        public string Username { get; set; } = default!;
        public string? AvatarUrl { get; set; }
    }
}
