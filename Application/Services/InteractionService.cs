using Application.Common.Result;
using Application.Dtos.Book;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services
{

    public class InteractionService : IInteractionService
    {
        private readonly IInteractionRepository _interactionRepo;
        private readonly IBookRepository _bookRepo;
        private readonly ICurrentUserService _currentUser;
        private readonly IOpenLibraryService _openLibrary;
        private readonly IMemoryCache _cache;
        private readonly IAchievementService _achievementService;

        public InteractionService(
            IInteractionRepository interactionRepo,
            IBookRepository bookRepo,
            ICurrentUserService currentUser,
            IOpenLibraryService openLibraryService,
            IMemoryCache cache,
            IAchievementService achievementService
            )
        {
            _interactionRepo = interactionRepo;
            _bookRepo = bookRepo;
            _currentUser = currentUser;
            _cache = cache;
            _openLibrary = openLibraryService;
            _achievementService = achievementService;
        }

        public async Task<Result> AddOrUpdate(CreateInteractionDto dto)
        {
            var result = new Result();

            var userId = _currentUser.UserId;

            if (userId == null)
            {
                result.IsSuccess = false;
                result.Message = "Usuario no autenticado";
                return result;
            }

            if (!Enum.IsDefined(typeof(InteractionStatus), dto.Status))
            {
                result.IsSuccess = false;
                result.Message = "Estado inválido";
                return result;
            }

            try
            {

                var book = await _bookRepo.GetByExternalId(dto.BookWorkKey);

                if (book == null)
                {
                    BookInfoDto? sourceBook = null;

                    var cacheKey = $"search_{dto.Query?.ToLower()}";

                    if (!string.IsNullOrWhiteSpace(dto.Query) &&
                        _cache.TryGetValue(cacheKey, out List<BookInfoDto> cachedBooks))
                    {
                        sourceBook = cachedBooks.FirstOrDefault(x => x.BookWorkKey == dto.BookWorkKey);
                    }

                    if (sourceBook == null)
                    {
                        sourceBook = await _openLibrary.GetByWork(dto.BookWorkKey);
                    }

                    if (sourceBook == null)
                    {
                        result.IsSuccess = false;
                        result.Message = "Libro inválido";
                        return result;
                    }

                    book = await _bookRepo.Create(new BookMetadata
                    {
                        Title = sourceBook.Title,
                        Author = sourceBook.Author ?? "Unknown",
                        Year = sourceBook.FirstPublishYear ?? 0,
                        BookWorkKey = dto.BookWorkKey,
                        CoverEditionKey = sourceBook.CoverEditionKey,
                        CreatedAt = DateTime.Now
                    });
                }

                var interaction = await _interactionRepo.GetByUserAndBook(userId, book.Id);

                var finishedDate = dto.Status == InteractionStatus.completed
                    ? dto.FinishedAt
                    : (DateTime?)null;

                if (interaction == null)
                {
                    interaction = new Interaction
                    {
                        UserId = userId,
                        BookId = book.Id,
                        Status = dto.Status,
                        FinishedAt = finishedDate,
                        CreatedAt = DateTime.UtcNow,
                        Rating = (dto.Rating > 0 && dto.Rating <= 5) ? dto.Rating : null
                    };

                    await _interactionRepo.Create(interaction);
                }
                else
                {
                    if(userId != interaction.UserId)
                    {
                        result.IsSuccess = false;
                        result.Message = "No tienes permiso para esta accion";
                        return result;
                    }

                    interaction.Status = dto.Status;
                    interaction.FinishedAt = finishedDate;
                    interaction.Rating = (dto.Rating > 0 && dto.Rating <= 5) ? dto.Rating : null;

                    await _interactionRepo.Update(interaction);
                }


                if (interaction.Status == InteractionStatus.completed)
                {
                    _ = _achievementService.CheckAndGrantAchievements(userId);
                }

                result.IsSuccess = true;
                result.Message = "Interacción guardada";

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Error al cargar el Inicio";
            }

            return result;
        }


        public async Task<Result<UserListsDto>> GetUserLists()
        {
            var userId = _currentUser.UserId;
            var result = new Result<UserListsDto>();

            try
            {
            
                var interactions = await _interactionRepo.GetByUser(userId);

                if (interactions == null || !interactions.Any())
                    return new Result<UserListsDto>();

                var bookIds = interactions.Select(x => x.BookId).Distinct().ToList();

                var books = await _bookRepo.GetByIds(bookIds);

                var bookDict = books.ToDictionary(x => x.Id);

            
                result.Data = new UserListsDto();

                foreach (var interaction in interactions)
                {
                    if (!bookDict.TryGetValue(interaction.BookId, out var book))
                        continue;

                    if (interaction.Status == InteractionStatus.reading)
                    {
                        result.Data.Reading.Add(new ReadingDto
                        {
                            Id = interaction.Id,
                            BookWorkKey = book.BookWorkKey!,
                            Title = book.Title,
                            Author = book.Author
                        });
                    }
                    else if (interaction.Status == InteractionStatus.completed)
                    {
                        result.Data.Completed.Add(new CompletedDto
                        {
                            Id = interaction.Id,
                            BookWorkKey = book.BookWorkKey!,
                            Title = book.Title,
                            Author = book.Author,
                            Rating = interaction.Rating,
                            FinishedAt = interaction.FinishedAt
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Error al cargar el Inicio";
            }

            return result;
        }

        public async Task<Result> MarkAsReaded(string id)
        {
            var result = new Result();

            try
            {
                var userId = _currentUser.UserId;

                if (userId == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Usuario no autenticado";
                    return result;
                }

                var interaction = await _interactionRepo.GetById(id);

                if (interaction == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Libro no encontrado.";
                    return result;
                }
                else
                {
                    if (userId != interaction.UserId)
                    {
                        result.IsSuccess = false;
                        result.Message = "No tienes permiso para esta accion";
                        return result;
                    }

                    interaction.Status = InteractionStatus.completed;
                    interaction.FinishedAt = null;


                    await _interactionRepo.Update(interaction);
                }

                result.IsSuccess = true;
                result.Message = "Libro marcado como completado";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Error al cargar el Inicio";
            }
            return result;
        }

        public async Task<Result> Delete(string id)
        {
            var result = new Result();

            try
            {
                var userId = _currentUser.UserId;

                if (userId == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Usuario no autenticado.";
                    return result;
                }
           
                var interaction = await _interactionRepo.GetById(id);

                if (interaction == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Libro no encontrado.";
                    return result;
                }
                else
                {
                    if (userId != interaction.UserId)
                    {
                        result.IsSuccess = false;
                        result.Message = "No tienes permiso para esta accion";
                        return result;
                    }

                    await _interactionRepo.Delete(interaction.Id);
                }

                result.IsSuccess = true;
                result.Message = "Libro eliminado de la lista.";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Error al cargar el Inicio";
            }
            return result;
        }

        public async Task<Result> EditReaded(EditReadedBookDto dto)
        {
            var result = new Result();

            try
            {
                var userId = _currentUser.UserId;

                if (userId == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Usuario no autenticado.";
                    return result;
                }


                var interaction = await _interactionRepo.GetById(dto.Id);

                if (interaction == null)
                {
                    result.IsSuccess = false;
                    result.Message = "Libro no encontrado.";
                    return result;
                }
                else
                {
                    if (userId != interaction.UserId)
                    {
                        result.IsSuccess = false;
                        result.Message = "No tienes permiso para esta accion";
                        return result;
                    }

                    interaction.FinishedAt = dto.Date;
                    interaction.Rating = (dto.Rating > 0 && dto.Rating <= 5) ? dto.Rating : null;

                    await _interactionRepo.Update(interaction);
                }

                result.IsSuccess = true;
                result.Message = "Datos editados.";
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Error al cargar el Inicio";
            }
            return result;
        }
    }

}