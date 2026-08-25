using Application.Dtos.Book;

namespace Application.Interfaces
{
    public interface IOpenLibraryService
    {
        Task<BookInfoDto?> GetByWork(string isbn);

        Task<List<BookInfoDto>> GetBooks(string search);

        Task<string?> GetCover(string isbn, string size);
    }
}
