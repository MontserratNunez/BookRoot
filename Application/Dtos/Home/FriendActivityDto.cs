using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Home
{
    public class FriendActivityDto
    {
        public string FriendUsername { get; set; }
        public string? FriendProfilePicture { get; set; }
        public string BookWorkKey { get; set; }
        public string BookTitle { get; set; }
        public int? Rating { get; set; }
        public System.DateTime FinishedAt { get; set; }
    }
}
