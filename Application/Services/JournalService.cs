using Application.Common.Result;
using Application.Dtos.Journal;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class JournalService : IJournalService
    {
        private readonly IInteractionRepository _interactionRepo;
        private readonly IBookRepository _bookRepo;
        private readonly IOpenLibraryService _openLibraryService;
        private readonly ICurrentUserService _currentUser;

        public JournalService(
            IInteractionRepository interactionRepo,
            IBookRepository bookRepo,
            IOpenLibraryService openLibraryService,
            ICurrentUserService currentUser)
        {
            _interactionRepo = interactionRepo;
            _bookRepo = bookRepo;
            _openLibraryService = openLibraryService;
            _currentUser = currentUser;
        }

        public async Task<Result<List<JournalItemDto>>> GetUserJournal()
        {
            var result = new Result<List<JournalItemDto>>();
            var journalItems = new List<JournalItemDto>();

            try
            {
                string? userId = _currentUser.UserId;

                if (string.IsNullOrEmpty(userId))
                {
                    result.IsSuccess = false;
                    result.Message = "Usuario no autenticado.";
                    return result;
                }

                var allInteractions = await _interactionRepo.GetByUser(userId);

                var completedInteractions = allInteractions
                    .Where(x => x.Status == InteractionStatus.completed && x.FinishedAt.HasValue)
                    .OrderByDescending(x => x.FinishedAt!.Value)
                    .ToList();

                foreach (var interaction in completedInteractions)
                {
                    var dbBook = await _bookRepo.GetById(interaction.BookId);
                    string title = dbBook?.Title ?? "Libro Desconocido";
                    string author = dbBook?.Author ?? "Desconocido";
                    string? coverEditionKey = dbBook?.CoverEditionKey;
                    string? bookWorkKey = dbBook?.BookWorkKey;

                    string? resolvedCoverUrl = null;
                    if (!string.IsNullOrEmpty(coverEditionKey))
                    {
                        resolvedCoverUrl = await _openLibraryService.GetCover(coverEditionKey, "S");
                    }

                    journalItems.Add(new JournalItemDto
                    {
                        InteractionId = interaction.Id,
                        BookWorkKey = bookWorkKey ?? "",
                        Title = title,
                        Author = author,
                        CoverUrl = resolvedCoverUrl,
                        Rating = interaction.Rating,
                        FinishedAt = interaction.FinishedAt!.Value
                    });
                }

                result.IsSuccess = true;
                result.Data = journalItems;
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