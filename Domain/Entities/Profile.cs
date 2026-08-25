using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Domain.Entities
{
    [Table("profiles")]
    public class Profile : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = default!;

        [Column("username")]
        public string Username { get; set; } = default!;

        [Column("bio")]
        public string? Bio { get; set; }

        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }

        [Column("top_four_ids")]
        public List<string>? TopFourIds { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }
    }
}
