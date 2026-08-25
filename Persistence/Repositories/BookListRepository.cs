using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Persistence.Repositories
{
    public class BookListRepository : IBookListRepository
    {
        private readonly ISupabaseClientProvider _provider;

        public BookListRepository(ISupabaseClientProvider provider)
        {
            _provider = provider;
        }

        public async Task<List<BookList>> GetByProfileId(string profileId)
        {
            var client = _provider.GetClient();
            var response = await client.From<BookList>()
                .Where(x => x.ProfileId == profileId)
                .Get();
            return response.Models;
        }

        public async Task<BookList?> GetById(string id)
        {
            var client = _provider.GetClient();
            var response = await client.From<BookList>()
                .Where(x => x.Id == id)
                .Single();
            return response;
        }

        public async Task<int> CountByProfileId(string profileId)
        {
            var client = _provider.GetClient();
            var response = await client.From<BookList>()
                .Where(x => x.ProfileId == profileId)
                .Get();
            return response.Models.Count;
        }

        public async Task<BookList> Create(BookList list)
        {
            var client = _provider.GetClient();
            var response = await client.From<BookList>().Insert(list);
            return response.Models.First();
        }

        public async Task<BookList> Update(BookList list)
        {
            var client = _provider.GetClient();
            var response = await client.From<BookList>().Update(list);
            return response.Models.First();
        }

        public async Task Delete(string id)
        {
            var client = _provider.GetClient();
            await client.From<BookList>().Where(x => x.Id == id).Delete();
        }
    }
}
