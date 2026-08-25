using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.Home
{
    public class HomeViewModel
    {
        public List<HomeBookViewModel> MostReadBooks { get; set; } = new();
        public List<HomeFriendActivityViewModel> FriendsActivity { get; set; } = new();
    }
}
