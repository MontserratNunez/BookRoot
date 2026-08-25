using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.User
{
    public class ProfileImageViewModel
    {
        public string? CurrentAvatarUrl { get; set; }
        public List<string> AvailableAvatars { get; set; } = new();
    }
}
