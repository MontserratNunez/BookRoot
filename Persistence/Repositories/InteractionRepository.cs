using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Supabase.Interfaces;

namespace Persistence.Repositories
{

    public class InteractionRepository : IInteractionRepository
    {
        private readonly ISupabaseClientProvider _provider;

        public InteractionRepository(ISupabaseClientProvider provider)
        {
            _provider = provider;
        }

        public async Task<Interaction?> GetById(string id)
        {
            var client = _provider.GetClient();

            var response = await client
                .From<Interaction>()
                .Where(x => x.Id == id)
                .Get();

            return response.Models.FirstOrDefault();
        }

        public async Task<List<Interaction?>> GetByBookId(string bookId)
        {
            var client = _provider.GetClient();

            var response = await client
                .From<Interaction>()
                .Where(x => x.BookId == bookId)
                .Get();

            return response.Models;
        }

        public async Task<List<Interaction?>> GetByUser(string userId)
        {
            var client = _provider.GetClient();

            var response = await client
                .From<Interaction>()
                .Where(x => x.UserId == userId)
                .Get();

            return response.Models;
        }

        public async Task<Interaction?> GetByUserAndBook(string userId, string bookId)
        {
            var client = _provider.GetClient();

            var response = await client
                .From<Interaction>()
                .Where(x => x.UserId == userId && x.BookId == bookId)
                .Get();

            return response.Models.FirstOrDefault();
        }

        public async Task Create(Interaction interaction)
        {
            var client = _provider.GetClient();
            await client.From<Interaction>().Insert(interaction);
        }

        public async Task Update(Interaction interaction)
        {
            var client = _provider.GetClient();
            await client.From<Interaction>().Update(interaction);
        }

        public async Task Delete(string id)
        {
            var client = _provider.GetClient();
            await client.From<Interaction>().Where(x => x.Id == id).Delete();
        }

        public async Task<List<Interaction>> GetCompletedBooksByUserId(string userId)
        {
            var client = _provider.GetClient();
            string statusTarget = InteractionStatus.completed.ToString();

            var response = await client.From<Interaction>()
                .Where(x => x.UserId == userId)
                .Filter("status", Supabase.Postgrest.Constants.Operator.Equals, statusTarget)
                .Get();
            return response.Models;
        }

        public async Task<List<Interaction>> GetAllCompletedInPeriod(DateTime start, DateTime end)
        {
            var client = _provider.GetClient();
            var response = await client
                .From<Interaction>()
                .Filter("status", Supabase.Postgrest.Constants.Operator.Equals, InteractionStatus.completed.ToString())
                .Where(x => x.FinishedAt >= start)
                .Where(x => x.FinishedAt <= end)
                .Get();

            return response.Models ?? new List<Interaction>();
        }
    }

}

