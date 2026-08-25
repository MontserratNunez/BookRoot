using Domain.Entities;
using Application.Dtos.Book;

namespace Application.Interfaces
{
    public interface IBookService
    {
        Task<List<BookInfoDto>> Search(string query);
        Task<BookDetailsDto?> GetDetails(string query, string bookKey);
    }
}

