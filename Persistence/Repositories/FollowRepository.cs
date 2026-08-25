using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Supabase.Postgrest;

namespace Persistence.Repositories
{
    public class FollowRepository : IFollowRepository
    {
        private readonly ISupabaseClientProvider _provider;

        public FollowRepository(ISupabaseClientProvider provider)
        {
            _provider = provider;
        }

        public async Task<Follow?> GetFollow(string followerId, string followingId)
        {
            var client = _provider.GetClient();
            var response = await client.From<Follow>()
                .Where(x => x.FollowerId == followerId && x.FollowingId == followingId)
                .Get();

            return response.Models.Count > 0 ? response.Models[0] : null;
        }

        public async Task<bool> AddFollow(Follow follow)
        {
            var client = _provider.GetClient();
            var response = await client.From<Follow>().Upsert(follow);
            return response.Models.Count > 0;
        }

        public async Task<bool> RemoveFollow(Follow follow)
        {
            var client = _provider.GetClient();
            await client.From<Follow>().Delete(follow);
            return true;
        }

        public async Task<int> GetFollows(string userId)
        {
            var client = _provider.GetClient();

            var response = await client.From<Follow>()
                .Where(x => x.FollowerId == userId)
                .Count(Constants.CountType.Exact);

            return response;
        }
        public async Task<int> GetFollowers(string userId)
        {
            var client = _provider.GetClient();

            var response = await client.From<Follow>()
                .Where(x => x.FollowingId == userId)
                .Count(Constants.CountType.Exact);

            return response;
        }

        public async Task<List<string>> GetAcceptedFriendsIds(string currentUserId)
        {
            var client = _provider.GetClient();

            var response = await client.From<Follow>()
                .Where(x => x.FollowerId == currentUserId)
                .Get();
            return response.Models.Select(x => x.FollowingId).ToList();
            
        }
    }
}