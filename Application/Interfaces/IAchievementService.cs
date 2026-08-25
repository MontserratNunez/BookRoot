using Application.Common.Result;
using Application.Dtos.Achievements;

namespace Application.Interfaces
{
    public interface IAchievementService
    {
        Task<Result<List<AchievementDto>>> GetUserAchievements(string profileId);
        /// <summary>
        /// Checks all achievement milestones for a user and grants any newly earned ones.
        /// Should be called after any interaction that could trigger a new achievement (e.g., completing a book).
        /// </summary>
        Task CheckAndGrantAchievements(string userId);
    }
}
