using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Persistence.Repositories
{
    public class AchievementRepository : IAchievementRepository
    {
        private readonly ISupabaseClientProvider _provider;

        public AchievementRepository(ISupabaseClientProvider provider)
        {
            _provider = provider;
        }

        public async Task<List<Achievement>> GetAll()
        {
            var client = _provider.GetClient();
            var response = await client.From<Achievement>().Get();
            return response.Models;
        }

        public async Task<Achievement?> GetByName(string name)
        {
            var client = _provider.GetClient();
            var response = await client.From<Achievement>()
                .Where(x => x.AchievementName == name)
                .Single();
            return response;
        }

        public async Task<List<UserAchievement>> GetUserAchievements(string profileId)
        {
            var client = _provider.GetClient();
            var response = await client.From<UserAchievement>()
                .Where(x => x.ProfileId == profileId)
                .Get();
            return response.Models;
        }

        public async Task<UserAchievement?> GetUserAchievement(string profileId, string achievementId)
        {
            var client = _provider.GetClient();
            var response = await client.From<UserAchievement>()
                .Where(x => x.ProfileId == profileId && x.AchievementId == achievementId)
                .Single();
            return response;
        }

        public async Task GrantAchievement(UserAchievement userAchievement)
        {
            var client = _provider.GetClient();
            await client.From<UserAchievement>().Insert(userAchievement);
        }

        public async Task<int> CountCompletedByUser(string userId)
        {
            var client = _provider.GetClient();
            var statusTarget = InteractionStatus.completed.ToString();
            var response = await client.From<Interaction>()
                .Where(x => x.UserId == userId)
                .Filter("status", Supabase.Postgrest.Constants.Operator.Equals, statusTarget)
                .Get();
            return response.Models.Count;
        }
    }
}
