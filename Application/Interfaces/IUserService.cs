using Application.Common.Result;
using Application.Dtos.Book;
using Application.Dtos.User;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<Result<List<UserSearchDto>>> SearchUsers(string query);
        Task<Result<UserProfileDto>> GetProfileByUsername(string? username);
        Task<Result<UpdateProfileDto>> GetProfileForEdit();
        Task<Result> UpdateProfile(UpdateProfileDto dto);
        Task<Result<List<string>>> GetAvailableAvatars();
        Task<Result<bool>> UpdateProfileImage(string avatarName);
        Task<Result<List<UserCompletedDto>>> GetCompletedBooks(string username);
        Task<Result> UpdateTopFour(UpdateTopFourDto dto);
        Task<Result<List<TopFourBookItemDto>>> GetTopFour(string? username);
        Task<Result> AddBookToTopFour(string bookWorkKey, int slotIndex);
        Task<Result> RemoveBookFromTopFour(string bookWorkKey);
        Task<Result> DeleteAccount();
    }
}
