using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IAchievementRepository
    {
        Task<List<Achievement>> GetAll();
        Task<Achievement?> GetByName(string name);
        Task<List<UserAchievement>> GetUserAchievements(string profileId);
        Task<UserAchievement?> GetUserAchievement(string profileId, string achievementId);
        Task GrantAchievement(UserAchievement userAchievement);
        Task<int> CountCompletedByUser(string userId);
    }
}
