using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IBookListRepository
    {
        Task<List<BookList>> GetByProfileId(string profileId);
        Task<BookList?> GetById(string id);
        Task<int> CountByProfileId(string profileId);
        Task<BookList> Create(BookList list);
        Task<BookList> Update(BookList list);
        Task Delete(string id);
    }
}
