using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Home
{
    public class HomeFriendActivityViewModel
    {
        public string FriendUsername { get; set; } = default!;
        public string FriendProfilePicture { get; set; } = default!;
        public string BookWorkKey { get; set; } = default!;
        public string BookTitle { get; set; } = default!;
        public string StarsRating { get; set; } = default!;
        public string TimeAgo { get; set; } = default!;
    }
}
