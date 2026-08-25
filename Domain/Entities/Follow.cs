using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Domain.Entities
{
    [Table("follows")]
    public class Follow : BaseModel
    {
        [PrimaryKey("follower_id")]
        public string FollowerId { get; set; } = default!;

        [PrimaryKey("following_id")]
        public string FollowingId { get; set; } = default!;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
