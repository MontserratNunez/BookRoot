
using Application.Common.Result;
using Application.Dtos.Book;

namespace Application.Interfaces
{

    public interface IInteractionService
    {
        Task<Result> AddOrUpdate(CreateInteractionDto dto);
        Task<Result<UserListsDto>> GetUserLists();
        Task<Result> EditReaded(EditReadedBookDto dto);
        Task<Result> MarkAsReaded(string bookKey);
        Task<Result> Delete(string bookKey);
    }

}
