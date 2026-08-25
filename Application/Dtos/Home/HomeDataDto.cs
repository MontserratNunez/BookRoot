using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Home
{
    public class HomeDataDto
    {
        public List<MostReadBookDto> MostReadBooks { get; set; } = new();
        public List<FriendActivityDto> FriendsActivity { get; set; } = new();
    }
}
