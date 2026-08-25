namespace Application.Dtos.Achievements
{
    public class AchievementDto
    {
        public string Id { get; set; } = default!;
        public string AchievementName { get; set; } = default!;
        public string? AchievementPhotoUrl { get; set; }
        public DateTime? EarnedAt { get; set; }
    }
}
