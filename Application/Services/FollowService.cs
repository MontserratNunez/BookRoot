using Application.Common.Result;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace Application.Services
{
    public class FollowService : IFollowService
    {
        private readonly IFollowRepository _followRepo;
        private readonly IUserRepository _userRepo;
        private readonly ICurrentUserService _currentUser;

        public FollowService(
            IFollowRepository followRepo,
            IUserRepository userRepo,
            ICurrentUserService currentUser)
        {
            _followRepo = followRepo;
            _userRepo = userRepo;
            _currentUser = currentUser;
        }

        public async Task<Result> ToggleFollow(string targetUsername)
        {
            var result = new Result();
            var followerId = _currentUser.UserId;

            if (string.IsNullOrEmpty(followerId))
            {
                result.IsSuccess = false;
                result.Message = "Sesión de usuario inválida.";
                return result;
            }
            
            try
            {
                var targetUser = await _userRepo.GetProfileByUsername(targetUsername);

                if (targetUser == null)
                {
                    result.IsSuccess = false;
                    result.Message = "El usuario que intenta seguir no existe.";
                    return result;
                }

                var followingId = targetUser.Id;

                if (followerId == followingId)
                {
                    result.IsSuccess = false;
                    result.Message = "No puedes realizar esta acción sobre tu propio perfil.";
                    return result;
                }

                var existingFollow = await _followRepo.GetFollow(followerId, followingId);

                if (existingFollow != null)
                {
                    await _followRepo.RemoveFollow(existingFollow);
                    result.IsSuccess = true;
                    result.Message = $"Has dejado de seguir a {targetUsername}.";
                }
                else
                {
                    var newFollow = new Follow
                    {
                        FollowerId = followerId,
                        FollowingId = followingId,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _followRepo.AddFollow(newFollow);
                    result.IsSuccess = true;
                    result.Message = $"Ahora sigues a {targetUsername}.";
                }
            }
            
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Ocurrió un error inesperado al procesar la solicitud. {followerId} - {targetUsername}";
            }

            return result;
        }
    }
}