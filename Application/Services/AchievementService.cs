using Application.Common.Result;
using Application.Dtos.Achievements;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class AchievementService : IAchievementService
    {
        private readonly IAchievementRepository _achievementRepo;

        private static readonly Dictionary<int, string> Milestones = new()
        {
            { 1,   "Primera Lectura" },
            { 5,   "Lector Constante" },
            { 10,  "Club de los 10" },
            { 25,  "Ratón de Biblioteca" },
            { 50,  "Bibliófilo" },
            { 100, "Centenario de Páginas" },
        };

        public AchievementService(IAchievementRepository achievementRepo)
        {
            _achievementRepo = achievementRepo;
        }

        public async Task<Result<List<AchievementDto>>> GetUserAchievements(string profileId)
        {
            var result = new Result<List<AchievementDto>>();
            try
            {
                var userAchievements = await _achievementRepo.GetUserAchievements(profileId);
                var allAchievements = await _achievementRepo.GetAll();
                var achievementDict = allAchievements.ToDictionary(a => a.Id);

                result.Data = userAchievements
                    .Where(ua => achievementDict.ContainsKey(ua.AchievementId))
                    .Select(ua => new AchievementDto
                    {
                        Id = ua.AchievementId,
                        AchievementName = achievementDict[ua.AchievementId].AchievementName,
                        AchievementPhotoUrl = achievementDict[ua.AchievementId].AchievementPhotoUrl,
                        EarnedAt = ua.Date
                    })
                    .OrderByDescending(a => a.EarnedAt)
                    .ToList();

                result.IsSuccess = true;
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al obtener los logros.";
            }
            return result;
        }

        public async Task CheckAndGrantAchievements(string userId)
        {
            try
            {
                var completedCount = await _achievementRepo.CountCompletedByUser(userId);
                var userAchievements = await _achievementRepo.GetUserAchievements(userId);
                var earnedIds = userAchievements.Select(ua => ua.AchievementId).ToHashSet();
                var allAchievements = await _achievementRepo.GetAll();
                var achievementByName = allAchievements.ToDictionary(a => a.AchievementName);

                foreach (var milestone in Milestones)
                {
                    if (completedCount < milestone.Key) continue;
                    if (!achievementByName.TryGetValue(milestone.Value, out var achievement)) continue;
                    if (earnedIds.Contains(achievement.Id)) continue;

                    await _achievementRepo.GrantAchievement(new UserAchievement
                    {
                        ProfileId = userId,
                        AchievementId = achievement.Id,
                        Date = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AchievementService] Error checking achievements: {ex.Message}");
            }
        }
    }
}
