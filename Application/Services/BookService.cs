using Application.Dtos.Book;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Services
{
    public class BookService: IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IInteractionRepository _interactionRepo;
        private readonly IOpenLibraryService _openLibrary;
        private readonly ICurrentUserService _currentUser;
        private readonly IMemoryCache _cache;
        private readonly IUserRepository _userRepository;

        public BookService(IBookRepository bookRepository, 
            IOpenLibraryService openLibrary, 
            ICurrentUserService currentUser,
            IMemoryCache cache,
            IInteractionRepository interactionRepo,
            IUserRepository userRepository) 
        {
            _bookRepository = bookRepository;
            _interactionRepo = interactionRepo;
            _openLibrary = openLibrary;
            _currentUser = currentUser;
            _cache = cache;
            _userRepository = userRepository;
        }

        public async Task<List<BookInfoDto>> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<BookInfoDto>();

            if (query.Length < 2)
                return new List<BookInfoDto>();

            var cacheKey = $"search_{query.ToLower()}";


            if (_cache.TryGetValue(cacheKey, out List<BookInfoDto> cached))
            {
                return cached;
            }

            var result = await _openLibrary.GetBooks(query);

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

            return result;
        }


        public async Task<BookDetailsDto?> GetDetails(string? query, string bookKey)
        {
            if (string.IsNullOrWhiteSpace(bookKey))
                return null;

            BookInfoDto? book = null;
            var cacheKey = "";

            if (!string.IsNullOrWhiteSpace(query))
            {
                cacheKey = $"search_{query.ToLower()}";
                if (_cache.TryGetValue(cacheKey, out List<BookInfoDto> cachedBooks))
                {
                    book = cachedBooks.FirstOrDefault(x => x.BookWorkKey == bookKey);
                }
            }
            else
            {
                cacheKey = $"search_{bookKey.ToLower()}";
                _cache.TryGetValue(cacheKey, out book);
            }

            var dbBook = await _bookRepository.GetByExternalId(bookKey);

            if (book == null && dbBook != null)
            {
                book = new BookInfoDto
                {
                    Title = dbBook.Title,
                    Author = dbBook.Author,
                    FirstPublishYear = dbBook.Year,
                    BookWorkKey = dbBook.BookWorkKey,
                    CoverEditionKey = dbBook.CoverEditionKey,
                };
            }

            if (book == null)
            {
                book = await _openLibrary.GetByWork(bookKey);

                if (book != null)
                {
                    _cache.Set(cacheKey, book, TimeSpan.FromMinutes(10));
                }
            }

            if (book == null)
                return null;

            var cover = await _openLibrary.GetCover(book.CoverEditionKey, "M");

            List<Interaction> interactions = new();

            if (dbBook != null)
            {
                interactions = await _interactionRepo.GetByBookId(dbBook.Id);
            }

            var ratings = interactions
                .Where(x => x.Rating.HasValue)
                .Select(x => x.Rating!.Value)
                .ToList();

            double? avg = ratings.Any() ? ratings.Average() : null;
            var userId = _currentUser.UserId;
            var userInteraction = interactions.FirstOrDefault(x => x.UserId == userId);

            bool isFavorite = false;
            int favoriteSlotIndex = -1;
            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    var profile = await _userRepository.GetProfileForEdit(userId);
                    if (profile?.TopFourIds != null)
                    {
                        favoriteSlotIndex = profile.TopFourIds.IndexOf(bookKey);
                        isFavorite = favoriteSlotIndex >= 0;
                    }
                }
                catch { }
            }

            return new BookDetailsDto
            {
                Title = book.Title,
                Author = book.Author,
                Year = book.FirstPublishYear,
                CoverEditionKey = cover,
                BookWorkKey = bookKey,
                AverageRating = avg,
                IsInReading = userInteraction?.Status == InteractionStatus.reading,
                IsCompleted = userInteraction?.Status == InteractionStatus.completed,
                SelfRating = userInteraction?.Rating,
                SelfFinished = userInteraction?.FinishedAt,
                IntId = userInteraction?.Id,
                IsFavorite = isFavorite,
                FavoriteSlotIndex = favoriteSlotIndex
            };
        }
    }
}
