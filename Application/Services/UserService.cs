using Application.Common.Result;
using Application.Dtos.Book;
using Application.Dtos.User;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IFollowRepository _followRepo;
        private readonly IInteractionRepository _interactionRepository;
        private readonly IBookRepository _bookRepo;
        private readonly IOpenLibraryService _openLibraryService;
        private readonly IAuthenticationRepository _authRepo;

        public UserService(IUserRepository userRepository, 
            ICurrentUserService currentUser, 
            IFollowRepository followRepository, 
            IInteractionRepository interactionRepository,
            IBookRepository bookRepository,
            IOpenLibraryService openLibraryService,
            IAuthenticationRepository authRepository
            )
        {
            _userRepository = userRepository;
            _currentUser = currentUser;
            _followRepo = followRepository;
            _interactionRepository = interactionRepository;
            _bookRepo = bookRepository;
            _openLibraryService = openLibraryService;
            _authRepo = authRepository;
        }

        public async Task<Result<List<UserSearchDto>>> SearchUsers(string query)
        {
            var result = new Result<List<UserSearchDto>>();

            if (string.IsNullOrWhiteSpace(query))
            {
                result.IsSuccess = false;
                result.Message = "Debe ingresar una busqueda";
                return result;
            }

            try
            {
                var response = await _userRepository.SearchUsers(query);

                var usersDto = response.Select(u => new UserSearchDto
                {
                    Username = u.Username,
                    AvatarUrl = u.AvatarUrl
                }).ToList();

                result.IsSuccess = true;
                result.Data = usersDto;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = "Ocurrió un error al buscar los usuarios.";
            }

            return result;
        }

        public async Task<Result<UserProfileDto>> GetProfileByUsername(string? username)
        {
            var result = new Result<UserProfileDto>();
            var userId = _currentUser.UserId;
            Profile? response = null;

            if (string.IsNullOrWhiteSpace(username))
            {
                if (string.IsNullOrEmpty(userId))
                {
                    result.IsSuccess = false;
                    result.Message = "Usuario no especificado o no autenticado.";
                    return result;
                }

                response = await _userRepository.GetProfileById(userId);
            }
            else
            {
                response = await _userRepository.GetProfileByUsername(username);
            }

            if (response == null)
            {
                result.IsSuccess = false;
                result.Message = "Usuario no encontrado.";
                return result;
            }

            bool isOwner = (!string.IsNullOrEmpty(userId) && response.Id == userId);

            bool follows = false;
            if (!string.IsNullOrEmpty(userId))
            {
                follows = await _followRepo.GetFollow(userId, response.Id) != null;
            }

            var following = await _followRepo.GetFollows(response.Id);
            var followers = await _followRepo.GetFollowers(response.Id);

            var profileDto = new UserProfileDto
            {
                Id = response.Id,
                Username = response.Username,
                AvatarUrl = response.AvatarUrl,
                Bio = response.Bio,
                IsOwner = isOwner,
                Follows = follows,
                Followers = followers,
                Following = following
            };

            result.IsSuccess = true;
            result.Data = profileDto;
            return result;
        }

        public async Task<Result<UpdateProfileDto>> GetProfileForEdit()
        {
            var result = new Result<UpdateProfileDto>();
            var userId = _currentUser.UserId;
            try
            {
                var response = await _userRepository.GetProfileForEdit(userId);

                if (response == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Perfil no encontrado.";
                    return result;
                }

                result.IsSuccess = true;
                result.Data = new UpdateProfileDto
                {
                    Username = response.Username,
                    Bio = response.Bio,
                    AvatarUrl = response.AvatarUrl,
                };
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al cargar los datos del perfil.";
            }
            return result;
        }

        public async Task<Result> UpdateProfile(UpdateProfileDto dto)
        {
            var result = new Result();
            var userId = _currentUser.UserId;

            if (string.IsNullOrWhiteSpace(dto.Username))
            {
                result.IsSuccess = false;
                result.Message = "El nombre de usuario no puede estar vacío.";
                return result;
            }

            try
            {
                var existingUserResponse = await _userRepository.UsernameExists(dto.Username, userId);

                if (existingUserResponse)
                {
                    result.IsSuccess = false;
                    result.Message = "El nombre de usuario ya está en uso.";
                    return result;
                }

                var profileResponse = await _userRepository.GetProfileForEdit(userId);

                if (profileResponse == null)
                {
                    result.IsSuccess = false;
                    result.Message = "El perfil original no existe.";
                    return result;
                }

                profileResponse.Username = dto.Username.Trim();
                profileResponse.Bio = dto.Bio?.Trim();

                await _userRepository.UpdateProfile(profileResponse);

                result.IsSuccess = true;
                result.Message = "Perfil actualizado correctamente.";
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Ocurrió un error inesperado al actualizar el perfil.";
            }

            return result;
        }

        public async Task<Result<List<string>>> GetAvailableAvatars()
        {
            var result = new Result<List<string>>();
            try
            {
                var avatars = new List<string>
                {
                    "hippo.png", "kangaroo.png", "lion.png",
                    "llama.png", "moose.png", "penguin.png", "polar-bear.png",
                    "snowy-owl.png", "walrus.png", "default-avatar.png"
                };

                result.IsSuccess = true;
                result.Data = avatars;
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al cargar el catálogo de avatares.";
            }
            return result;
        }

        public async Task<Result<bool>> UpdateProfileImage(string avatarName)
        {
            var result = new Result<bool>();
            if (string.IsNullOrWhiteSpace(avatarName))
            {
                result.IsSuccess = false;
                result.Message = "Debe seleccionar un avatar válido.";
                return result;
            }

            var userId = _currentUser.UserId;

            try
            {
                var profileResponse = await _userRepository.GetProfileForEdit(userId);

                if (profileResponse == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Perfil no encontrado.";
                    return result;
                }

                profileResponse.AvatarUrl = $"/images/profile-images/{avatarName}";

                await profileResponse.Update<Profile>();

                result.IsSuccess = true;
                result.Message = "Imagen de perfil actualizada con éxito.";
                result.Data = true;
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al guardar los cambios en la base de datos.";
                result.Data = false;
            }
            return result;
        }

        public async Task<Result<List<UserCompletedDto>>> GetCompletedBooks(string username)
        {
            var result = new Result<List<UserCompletedDto>>();
            result.Data = new List<UserCompletedDto>();
            
            try
            {
                var user = await _userRepository.GetProfileByUsername(username);
                if (user == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Usuario no encontrado.";
                    return result;
                }

                var interactions = await _interactionRepository.GetCompletedBooksByUserId(user.Id);

                var bookIds = interactions.Select(x => x.BookId).Distinct().ToList();

                var books = await _bookRepo.GetByIds(bookIds);

                var bookDict = books.ToDictionary(x => x.Id);

                if (interactions == null || !interactions.Any())
                    return new Result<List<UserCompletedDto>>() { IsSuccess = true, Message = "Este perfil no tiene libros completados."};

                foreach (var interaction in interactions)
                {
                    if (!bookDict.TryGetValue(interaction.BookId, out var book))
                        continue;

                    result.Data.Add(new UserCompletedDto
                    {
                        BookWorkKey = book.BookWorkKey!,
                        Title = book.Title,
                        Author = book.Author,
                        Rating = interaction.Rating,
                    });

                }

                result.IsSuccess = true;
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al cargar los libros leídos.";
            }
            return result;
        }

        public async Task<Result<List<TopFourBookItemDto>>> GetTopFour(string? username)
        {
            var result = new Result<List<TopFourBookItemDto>>();

            Profile profile = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(username))
                {
                    profile = await _userRepository.GetProfileByUsername(username);
                }
                else
                {
                    var userId = _currentUser.UserId;

                    profile = await _userRepository.GetProfileForEdit(userId);
                }

                if (profile == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Perfil no encontrado";
                    return result;
                }

                var currentTopKeys = profile.TopFourIds ?? new List<string>();
                var topFourBooks = new List<TopFourBookItemDto>();

                foreach (var key in currentTopKeys)
                {
                    if (string.IsNullOrWhiteSpace(key)) continue;

                    var dbBook = await _bookRepo.GetByExternalId(key);

                    string? coverEditionKey = dbBook?.CoverEditionKey;
                    string title = dbBook?.Title ?? "Libro Desconocido";

                    if (dbBook == null)
                    {
                        try
                        {
                            var openLibraryBook = await _openLibraryService.GetByWork(key);
                            if (openLibraryBook != null)
                            {
                                coverEditionKey = openLibraryBook.CoverEditionKey;
                                title = openLibraryBook.Title;
                            }
                        }
                        catch { }
                    }

                    string? resolvedCoverUrl = null;
                    if (!string.IsNullOrEmpty(coverEditionKey))
                    {
                        resolvedCoverUrl = await _openLibraryService.GetCover(coverEditionKey, "M");
                    }

                    topFourBooks.Add(new TopFourBookItemDto
                    {
                        BookWorkKey = key,
                        Title = title,
                        CoverUrl = resolvedCoverUrl
                    });
                }

                result.IsSuccess = true;
                result.Data = topFourBooks;
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Ocurrió un error inesperado en el servidor.";
            }
            return result;
        }


        public async Task<Result> UpdateTopFour(UpdateTopFourDto dto)
        {
            var result = new Result();
            var userId = _currentUser.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                result.IsSuccess = false;
                result.Message = "Sesión inválida.";
                return result;
            }

            var cleanKeys = dto.BookKeys?
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Take(4)
                .ToList() ?? new List<string>();

            try
            {
                var profile = await _userRepository.GetProfileForEdit(userId);
                if (profile == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Perfil no encontrado.";
                    return result;
                }

                profile.TopFourIds = cleanKeys;

                var updated = await _userRepository.UpdateProfile(profile);
                if (!updated)
                {
                    result.IsSuccess = false;
                    result.Message = "No se pudieron guardar los cambios en la base de datos.";
                    return result;
                }

                result.IsSuccess = true;
                result.Message = "Top 4 de libros actualizado con éxito.";
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Ocurrió un error inesperado al procesar la solicitud.";
            }

            return result;
        }


        public async Task<Result> AddBookToTopFour(string bookWorkKey, int slotIndex)
        {
            var result = new Result();
            var userId = _currentUser.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                result.IsSuccess = false;
                result.Message = "Sesión inválida.";
                return result;
            }

            if (string.IsNullOrWhiteSpace(bookWorkKey))
            {
                result.IsSuccess = false;
                result.Message = "Clave del libro inválida.";
                return result;
            }

            try
            {
                var profile = await _userRepository.GetProfileForEdit(userId);
                if (profile == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Perfil no encontrado.";
                    return result;
                }

                var topFour = profile.TopFourIds ?? new List<string>();

                while (topFour.Count < 4) topFour.Add("");

                if (slotIndex < 0 || slotIndex > 3)
                {
                    int emptySlot = topFour.IndexOf("");
                    slotIndex = emptySlot >= 0 ? emptySlot : 0;
                }

                topFour[slotIndex] = bookWorkKey;
                profile.TopFourIds = topFour.Where(k => !string.IsNullOrWhiteSpace(k)).ToList();

                var orderedKeys = new List<string>();
                for (int i = 0; i < 4; i++)
                {
                    var key = i < topFour.Count ? topFour[i] : "";
                    if (!string.IsNullOrWhiteSpace(key)) orderedKeys.Add(key);
                }
                var finalList = new List<string> { "", "", "", "" };
                for (int i = 0; i < 4; i++)
                    finalList[i] = i < topFour.Count ? topFour[i] : "";

                profile.TopFourIds = finalList.Where(k => !string.IsNullOrWhiteSpace(k)).ToList();

                var updated = await _userRepository.UpdateProfile(profile);
                if (!updated)
                {
                    result.IsSuccess = false;
                    result.Message = "No se pudieron guardar los cambios.";
                    return result;
                }

                result.IsSuccess = true;
                result.Message = "Libro agregado a favoritos exitosamente.";
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Ocurrió un error inesperado.";
            }

            return result;
        }

        public async Task<Result> RemoveBookFromTopFour(string bookWorkKey)
        {
            var result = new Result();
            var userId = _currentUser.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                result.IsSuccess = false;
                result.Message = "Sesión inválida.";
                return result;
            }

            try
            {
                var profile = await _userRepository.GetProfileForEdit(userId);
                if (profile == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Perfil no encontrado.";
                    return result;
                }

                var topFour = profile.TopFourIds ?? new List<string>();
                topFour.Remove(bookWorkKey);
                profile.TopFourIds = topFour;

                var updated = await _userRepository.UpdateProfile(profile);
                if (!updated)
                {
                    result.IsSuccess = false;
                    result.Message = "No se pudieron guardar los cambios.";
                    return result;
                }

                result.IsSuccess = true;
                result.Message = "Libro eliminado de favoritos exitosamente.";
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Ocurrió un error inesperado.";
            }

            return result;
        }

        public async Task<Result> DeleteAccount()
        {
            var result = new Result();
            var userId = _currentUser.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                result.IsSuccess = false;
                result.Message = "Sesión inválida.";
                return result;
            }

            try
            {
                
                var deleted = await _authRepo.DeleteAccount(userId);

                if (!deleted)
                {
                    result.IsSuccess = false;
                    result.Message = "No se pudo eliminar la cuenta. Intenta de nuevo más tarde.";
                    return result;
                }

                result.IsSuccess = true;
                result.Message = "Tu cuenta ha sido eliminada permanentemente.";
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al eliminar la cuenta.";
            }

            return result;
        }
    }
}
