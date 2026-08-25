using Application.ViewModels.Book;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModels.User
{
    public class UserProfileViewModel
    {
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
        public string Bio {  get; set; }
        public bool IsOwner { get; set; }
        public bool Follows { get; set; }
        public int Following { get; set; }
        public int Followers { get; set; }
        
    }
}
