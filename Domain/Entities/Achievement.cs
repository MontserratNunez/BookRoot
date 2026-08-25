using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Domain.Entities
{
    [Table("achievement")]
    public class Achievement : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = default!;

        [Column("achievement_name")]
        public string AchievementName { get; set; } = default!;

        [Column("achievement_photo_url")]
        public string? AchievementPhotoUrl { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }
    }
}
