using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IBookRepository
    {
        Task<BookMetadata> Create(BookMetadata book);
        Task<BookMetadata?> GetById(string id);
        Task<BookMetadata?> GetByExternalId(string externalId);
        Task Delete(string id);
        Task<List<BookMetadata>> GetByIds(List<string> ids);
    }
}