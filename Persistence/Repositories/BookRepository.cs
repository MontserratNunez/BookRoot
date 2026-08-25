using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Supabase;

namespace Persistence.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly Supabase.Client _client;
        public BookRepository(ISupabaseClientProvider provider)
        {
            _client = provider.GetClient();
        }

        public async Task<BookMetadata> Create(BookMetadata book)
        {
            var response = await _client.From<BookMetadata>().Insert(book);

            return response.Models.First();
        }

        public async Task<BookMetadata?> GetById(string id)
        {
            var response = await _client.From<BookMetadata>().Where(x => x.Id == id).Get();

            return response.Models.FirstOrDefault();
        }

        public async Task<BookMetadata?> GetByExternalId(string externalId)
        {
            var response = await _client
                .From<BookMetadata>()
                .Where(x => x.BookWorkKey == externalId)
                .Get();

            return response.Models.FirstOrDefault();
        }

        public async Task<List<BookMetadata>> GetByIds(List<string> ids)
        {
            var response = await _client
                .From<BookMetadata>()
                .Filter("id", Supabase.Postgrest.Constants.Operator.In, ids)
                .Get();

            return response.Models;
        }

        public async Task Delete(string id)
        {
            await _client.From<BookMetadata>().Where(x => x.Id == id).Delete();
        }
    }
}