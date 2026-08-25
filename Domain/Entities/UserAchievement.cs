using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Domain.Entities
{
    [Table("user_achievement")]
    public class UserAchievement : BaseModel
    {
        [PrimaryKey("profile_id", false)]
        [Column("profile_id")]
        public string ProfileId { get; set; } = default!;

        [PrimaryKey("achievement_id", false)]
        [Column("achievement_id")]
        public string AchievementId { get; set; } = default!;

        [Column("date")]
        public DateTime? Date { get; set; }
    }
}
