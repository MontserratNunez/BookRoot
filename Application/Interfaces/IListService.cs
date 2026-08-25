using Application.Common.Result;
using Application.Dtos.Lists;

namespace Application.Interfaces
{
    public interface IListService
    {
        Task<Result<List<BookListDto>>> GetUserLists(string username);
        Task<Result<BookListDto>> GetListById(string listId, string? viewerUsername);
        Task<Result> CreateList(CreateListDto dto);
        Task<Result> DeleteList(string listId);
        Task<Result> AddBookToList(AddBookToListDto dto);
        Task<Result> RemoveBookFromList(string listId, string bookWorkKey);
        Task<Result<List<BookListDto>>> GetMyLists(string username);
    }
}
