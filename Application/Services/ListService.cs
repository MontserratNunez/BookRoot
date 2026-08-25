using Application.Common.Result;
using Application.Dtos.Book;
using Application.Dtos.Lists;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class ListService : IListService
    {
        private const int MaxListsPerUser = 10;
        private const int MaxBooksPerList = 50;

        private readonly IBookListRepository _listRepo;
        private readonly IUserRepository _userRepo;
        private readonly IBookRepository _bookRepo;
        private readonly IOpenLibraryService _openLibrary;
        private readonly ICurrentUserService _currentUser;

        public ListService(
            IBookListRepository listRepo,
            IUserRepository userRepo,
            IBookRepository bookRepo,
            IOpenLibraryService openLibrary,
            ICurrentUserService currentUser)
        {
            _listRepo = listRepo;
            _userRepo = userRepo;
            _bookRepo = bookRepo;
            _openLibrary = openLibrary;
            _currentUser = currentUser;
        }

        public async Task<Result<List<BookListDto>>> GetUserLists(string username)
        {
            var result = new Result<List<BookListDto>>();
            try
            {
                var profile = await _userRepo.GetProfileByUsername(username);
                if (profile == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Usuario no encontrado.";
                    return result;
                }

                var lists = await _listRepo.GetByProfileId(profile.Id);
                result.IsSuccess = true;
                result.Data = lists.Select(l => new BookListDto
                {
                    Id = l.Id,
                    ListName = l.ListName,
                    ListDescription = l.ListDescription,
                    BooksIds = l.BooksIds ?? new List<string>(),
                    CreatedAt = l.CreatedAt,
                    ListOwnerUsername = profile.Username
                }).ToList();
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al obtener las listas.";
            }
            return result;
        }

        public async Task<Result<List<BookListDto>>> GetMyLists(string username)
        {
            var userId = _currentUser.UserId;
            var result = new Result<List<BookListDto>>();
            try
            {
                var lists = await _listRepo.GetByProfileId(userId);
                result.IsSuccess = true;
                result.Data = lists.Select(l => new BookListDto
                {
                    Id = l.Id,
                    ListName = l.ListName,
                    ListDescription = l.ListDescription,
                    BooksIds = l.BooksIds ?? new List<string>(),
                    CreatedAt = l.CreatedAt,
                    ListOwnerUsername = username
                }).ToList();
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al obtener tus listas.";
            }
            return result;
        }

        public async Task<Result<BookListDto>> GetListById(string listId, string? viewerUsername)
        {
            var result = new Result<BookListDto>();
            try
            {
                var list = await _listRepo.GetById(listId);
                if (list == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Lista no encontrada.";
                    return result;
                }

                var userId = list.ProfileId;
                var profile = await _userRepo.GetProfileById(userId);

                result.IsSuccess = true;
                result.Data = new BookListDto
                {
                    Id = list.Id,
                    ListName = list.ListName,
                    ListDescription = list.ListDescription,
                    BooksIds = list.BooksIds ?? new List<string>(),
                    CreatedAt = list.CreatedAt,
                    ListOwnerUsername = profile.Username
                };
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al obtener la lista.";
            }
            return result;
        }

        public async Task<Result> CreateList(CreateListDto dto)
        {
            var result = new Result();
            var userId = _currentUser.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                result.IsSuccess = false;
                result.Message = "Sesión inválida.";
                return result;
            }

            if (string.IsNullOrWhiteSpace(dto.ListName))
            {
                result.IsSuccess = false;
                result.Message = "El nombre de la lista no puede estar vacío.";
                return result;
            }

            try
            {
                var count = await _listRepo.CountByProfileId(userId);
                if (count >= MaxListsPerUser)
                {
                    result.IsSuccess = false;
                    result.Message = $"No puedes tener más de {MaxListsPerUser} listas.";
                    return result;
                }

                var list = new BookList
                {
                    ProfileId = userId,
                    ListName = dto.ListName.Trim(),
                    ListDescription = dto.ListDescription?.Trim(),
                    BooksIds = new List<string>(),
                    CreatedAt = DateTime.UtcNow
                };

                await _listRepo.Create(list);
                result.IsSuccess = true;
                result.Message = "Lista creada con éxito.";
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al crear la lista.";
            }
            return result;
        }

        public async Task<Result> DeleteList(string listId)
        {
            var result = new Result();
            var userId = _currentUser.UserId;

            try
            {
                var list = await _listRepo.GetById(listId);
                if (list == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Lista no encontrada.";
                    return result;
                }

                if (list.ProfileId != userId)
                {
                    result.IsSuccess = false;
                    result.Message = "No tienes permiso para eliminar esta lista.";
                    return result;
                }

                await _listRepo.Delete(listId);
                result.IsSuccess = true;
                result.Message = "Lista eliminada.";
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al eliminar la lista.";
            }
            return result;
        }

        public async Task<Result> AddBookToList(AddBookToListDto dto)
        {
            var result = new Result();
            var userId = _currentUser.UserId;

            try
            {
                var list = await _listRepo.GetById(dto.ListId);
                if (list == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Lista no encontrada.";
                    return result;
                }

                if (list.ProfileId != userId)
                {
                    result.IsSuccess = false;
                    result.Message = "No tienes permiso para modificar esta lista.";
                    return result;
                }

                list.BooksIds ??= new List<string>();

                if (list.BooksIds.Count >= MaxBooksPerList)
                {
                    result.IsSuccess = false;
                    result.Message = $"La lista ya alcanzó el máximo de {MaxBooksPerList} libros.";
                    return result;
                }

                if (list.BooksIds.Contains(dto.BookWorkKey))
                {
                    result.IsSuccess = false;
                    result.Message = "El libro ya está en esta lista.";
                    return result;
                }

                var book = await _bookRepo.GetByExternalId(dto.BookWorkKey);

                if (book == null)
                {
                    var sourceBook = await _openLibrary.GetByWork(dto.BookWorkKey);

                    if (sourceBook == null)
                    {
                        result.IsSuccess = false;
                        result.Message = "No se pudo obtener la información del libro.";
                        return result;
                    }

                    await _bookRepo.Create(new BookMetadata
                    {
                        Title = sourceBook.Title,
                        Author = sourceBook.Author ?? "Unknown",
                        Year = sourceBook.FirstPublishYear ?? 0,
                        BookWorkKey = dto.BookWorkKey,
                        CoverEditionKey = sourceBook.CoverEditionKey,
                        CreatedAt = DateTime.Now
                    });
                }

                list.BooksIds.Add(dto.BookWorkKey);
                await _listRepo.Update(list);

                result.IsSuccess = true;
                result.Message = "Libro agregado a la lista.";
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al agregar el libro.";
            }
            return result;
        }

        public async Task<Result> RemoveBookFromList(string listId, string bookWorkKey)
        {
            var result = new Result();
            var userId = _currentUser.UserId;

            try
            {
                var list = await _listRepo.GetById(listId);
                if (list == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Lista no encontrada.";
                    return result;
                }

                if (list.ProfileId != userId)
                {
                    result.IsSuccess = false;
                    result.Message = "No tienes permiso para modificar esta lista.";
                    return result;
                }

                list.BooksIds?.Remove(bookWorkKey);
                await _listRepo.Update(list);

                result.IsSuccess = true;
                result.Message = "Libro eliminado de la lista.";
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Message = "Error al eliminar el libro de la lista.";
            }
            return result;
        }
    }
}
