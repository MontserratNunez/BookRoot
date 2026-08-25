using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.User
{
    public class UpdateProfileDto
    {
        public string Username { get; set; } = default!;
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
