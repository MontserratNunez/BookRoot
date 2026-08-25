using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.User
{
    public class UserSearchViewModel
    {
        public string Username { get; set; } = default!;
        public string? AvatarUrl { get; set; }
    }
}
