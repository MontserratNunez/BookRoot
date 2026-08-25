using Application.Dtos.Book;

namespace Application.Dtos.User
{
    public class UserProfileDto
    {
        public string Id { get; set; } = default!;
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
        public string Bio { get; set; }
        public bool IsOwner { get; set; }
        public bool Follows { get; set; }

        public int Following {  get; set; }
        public int Followers { get; set; }

    }
}
