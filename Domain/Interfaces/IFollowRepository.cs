using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IFollowRepository
    {
        Task<Follow?> GetFollow(string followerId, string followingId);
        Task<bool> AddFollow(Follow follow);
        Task<bool> RemoveFollow(Follow follow);

        Task<int> GetFollows(string userId);
        Task<int> GetFollowers(string userId);
        Task<List<string>> GetAcceptedFriendsIds(string currentUserId);
    }
}
