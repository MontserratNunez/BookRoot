using Application.Common.Result;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.VisualBasic;
using Supabase;

namespace Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ISupabaseClientProvider _provider;

        public UserRepository(ISupabaseClientProvider provider)
        {
            _provider = provider;
        }

        public async Task<List<Profile?>> SearchUsers(string query)
        {
            var client = _provider.GetClient();

            var response = await client.From<Profile>()
                    .Filter("username", Supabase.Postgrest.Constants.Operator.ILike, $"%{query}%")
                    .Get();

            return response.Models;
        }

        public async Task<Profile?> GetProfileByUsername(string username)
        {
            var client = _provider.GetClient();

            var response = await client.From<Profile>()
                .Where(x => x.Username == username)
                .Single();

            return response;
        }

        public async Task<Profile?> GetProfileById(string id)
        {
            var client = _provider.GetClient();

            var response = await client.From<Profile>()
                .Where(x => x.Id == id)
                .Single();

            return response;
        }

        public async Task<Profile> GetProfileForEdit(string userId)
        {
            var client = _provider.GetClient();
            var response = await client.From<Profile>().Where(x => x.Id == userId).Single();

            return response;
        }

        public async Task<bool> UsernameExists(string username, string userId)
        {
            var client = _provider.GetClient();

            var existingUserResponse = await client.From<Profile>()
                .Where(x => x.Username == username && x.Id != userId)
                .Get();

            if (existingUserResponse.Models.Count > 0)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> UpdateProfile(Profile profile)
        {
            var client = _provider.GetClient();
            var response = await client.From<Profile>().Update(profile);
            return response.Models.Count > 0;
        }
    }
}
