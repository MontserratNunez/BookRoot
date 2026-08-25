using Domain.Entities;
namespace Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<List<Profile?>> SearchUsers(string query);
        Task<Profile?> GetProfileByUsername(string username);
        Task<Profile?> GetProfileById(string id);
        Task<Profile> GetProfileForEdit(string userId);
        Task<bool> UsernameExists(string username, string userId);
        Task<bool> UpdateProfile(Profile profile);
    }
}
