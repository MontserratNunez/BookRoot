using Application.Common.Result;
using Application.Dtos.Home;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class HomeService : IHomeService
    {
        private readonly IInteractionRepository _interactionRepo;
        private readonly IBookRepository _bookRepo;
        private readonly IFollowRepository _followRepo;
        private readonly IUserRepository _userRepository;
        private readonly IOpenLibraryService _openLibraryService;
        private readonly ICurrentUserService _currentUser;
        private readonly IMemoryCache _cache;

        private const string MostReadBooksCacheKey = "Home_MostReadBooks";

        public HomeService(
            IInteractionRepository interactionRepo,
            IBookRepository bookRepo,
            IFollowRepository followRepo,
            IUserRepository userRepository,
            IOpenLibraryService openLibraryService,
            ICurrentUserService currentUser,
            IMemoryCache cache)
        {
            _interactionRepo = interactionRepo;
            _bookRepo = bookRepo;
            _followRepo = followRepo;
            _userRepository = userRepository;
            _openLibraryService = openLibraryService;
            _currentUser = currentUser;
            _cache = cache;
        }

        public async Task<Result<HomeDataDto>> GetHomeDashboardData()
        {
            var result = new Result<HomeDataDto>();
            var dashboard = new HomeDataDto();
            var currentUserId = _currentUser.UserId;
            
            try
            {
            
                dashboard.MostReadBooks = await _cache.GetOrCreateAsync(
                    MostReadBooksCacheKey,
                    async entry =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7);
                        return await GetMostReadBooksFromDbAsync();
                    }) ?? new List<MostReadBookDto>();

                if (!string.IsNullOrEmpty(currentUserId))
                {
                    var friendsIds = await _followRepo.GetAcceptedFriendsIds(currentUserId);
                    var oneMonthAgo = DateTime.UtcNow.AddMonths(-1);

                    foreach (var friendId in friendsIds)
                    {
                        var friendProfile = await _userRepository.GetProfileById(friendId);
                        if (friendProfile == null) continue;

                        var friendInteractions = await _interactionRepo.GetByUser(friendId);

                        var recentCompleted = friendInteractions
                            .Where(x => x.Status == InteractionStatus.completed &&
                                       ((x.FinishedAt.HasValue && x.FinishedAt >= oneMonthAgo) ||
                                        (!x.FinishedAt.HasValue && x.CreatedAt >= oneMonthAgo)))
                            .ToList();

                        foreach (var inter in recentCompleted)
                        {
                            var book = await _bookRepo.GetById(inter.BookId);
                            if (book == null) continue;

                            dashboard.FriendsActivity.Add(new FriendActivityDto
                            {
                                FriendUsername = friendProfile.Username,
                                FriendProfilePicture = friendProfile.AvatarUrl,
                                BookWorkKey = book.BookWorkKey ?? "",
                                BookTitle = book.Title,
                                Rating = (inter.Rating > 0) ? inter.Rating : null,
                                FinishedAt = inter.FinishedAt ?? inter.CreatedAt.Value
                            });
                        }
                    }

                    dashboard.FriendsActivity = dashboard.FriendsActivity
                        .OrderByDescending(x => x.FinishedAt)
                        .ToList();
                }

                result.IsSuccess = true;
                result.Data = dashboard;
                
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Error al cargar el Inicio";
            }
                
            return result;
        }

        private async Task<List<MostReadBookDto>> GetMostReadBooksFromDbAsync()
        {
            var mostReadBooks = new List<MostReadBookDto>();
            var targetDate = DateTime.UtcNow;
            List<Interaction> targetInteractions = new();
            int attempts = 0;

            while (targetInteractions.Select(x => x.BookId).Distinct().Count() < 10 && attempts < 12)
            {
                var startOfMonth = new DateTime(targetDate.Year, targetDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

                var interactions = await _interactionRepo.GetAllCompletedInPeriod(startOfMonth, endOfMonth);
                if (interactions != null && interactions.Any())
                {
                    targetInteractions.AddRange(interactions);
                }

                targetDate = targetDate.AddMonths(-1);
                attempts++;
            }

            if (targetInteractions.Select(x => x.BookId).Distinct().Count() < 10)
            {
                var farPast = DateTime.UtcNow.AddYears(-10);
                var now = DateTime.UtcNow;

                var allCompleted = await _interactionRepo.GetAllCompletedInPeriod(farPast, now);
                if (allCompleted != null && allCompleted.Any())
                {
                    targetInteractions = allCompleted.ToList();
                }
            }

            var topBookGroups = targetInteractions
                .GroupBy(x => x.BookId)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToList();

            foreach (var group in topBookGroups)
            {
                var dbBook = await _bookRepo.GetById(group.Key);
                if (dbBook == null) continue;

                var allBookInteractions = await _interactionRepo.GetByBookId(dbBook.Id);
                var ratings = allBookInteractions?
                    .Where(x => x.Rating.HasValue && x.Rating > 0)
                    .Select(x => x.Rating!.Value)
                    .ToList() ?? new List<int>();

                double? avgRating = ratings.Any() ? Math.Round(ratings.Average(), 1) : null;

                string? resolvedCover = null;
                if (!string.IsNullOrEmpty(dbBook.CoverEditionKey))
                {
                    resolvedCover = await _openLibraryService.GetCover(dbBook.CoverEditionKey, "M");
                }
                if (string.IsNullOrEmpty(resolvedCover) && !string.IsNullOrEmpty(dbBook.BookWorkKey))
                {
                    resolvedCover = $"https://covers.openlibrary.org/b/olid/{dbBook.BookWorkKey}-M.jpg";
                }

                mostReadBooks.Add(new MostReadBookDto
                {
                    BookWorkKey = dbBook.BookWorkKey ?? "",
                    Title = dbBook.Title,
                    Author = dbBook.Author,
                    CoverUrl = resolvedCover,
                    AverageRating = avgRating,
                    ReadCount = group.Count()
                });
            }

            return mostReadBooks;
        }
    }
}