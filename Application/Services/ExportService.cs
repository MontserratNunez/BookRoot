using Application.Common.Result;
using Application.ExportLayouts;
using Application.Interfaces;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services
{
    public class ExportService : IExportService
    {
        private readonly IInteractionRepository _interactionRepo;
        private readonly IBookRepository _bookRepo;
        private readonly ICurrentUserService _currentUser;
        private readonly IUserRepository _userRepo;

        public ExportService(
            IInteractionRepository interactionRepo,
            IBookRepository bookRepo,
            ICurrentUserService currentUser,
            IUserRepository userRepo)
        {
            _interactionRepo = interactionRepo;
            _bookRepo = bookRepo;
            _currentUser = currentUser;
            _userRepo = userRepo;
        }

        public async Task<Result<string>> GenerateHtmlExport(string username, string filter, string theme = "floral", DateTime? from = null, DateTime? to = null)
        {
            var result = new Result<string>();
            var userId = _currentUser.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                result.IsSuccess = false;
                result.Message = "Sesión inválida.";
                return result;
            }

            try
            {
                var allInteractions = await _interactionRepo.GetCompletedBooksByUserId(userId);

                var now = DateTime.UtcNow;
                var filtered = filter switch
                {
                    "last_month" => allInteractions.Where(x => x.FinishedAt >= now.AddMonths(-1)).ToList(),
                    "last_year"  => allInteractions.Where(x => x.FinishedAt >= now.AddYears(-1)).ToList(),
                    "custom"     => allInteractions.Where(x => x.FinishedAt >= from && x.FinishedAt <= to).ToList(),
                    _            => allInteractions
                };

                var bookIds = filtered.Select(x => x.BookId).Distinct().ToList();
                var books = await _bookRepo.GetByIds(bookIds);
                var bookDict = books.ToDictionary(b => b.Id);

                var grouped = filtered
                    .Where(x => x.FinishedAt.HasValue && bookDict.ContainsKey(x.BookId))
                    .GroupBy(x => x.FinishedAt!.Value.Year)
                    .OrderByDescending(g => g.Key)
                    .ToList();

                var html = BuildHtml(username, grouped, bookDict, filter, theme, from, to);

                result.IsSuccess = true;
                result.Data = html;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Message = $"Error al generar el reporte: {ex.Message}";
            }

            return result;
        }

        private static string BuildHtml(
            string username,
            IEnumerable<IGrouping<int, Domain.Entities.Interaction>> grouped,
            Dictionary<string, Domain.Entities.BookMetadata> bookDict,
            string filter,
            string theme,
            DateTime? from,
            DateTime? to)
        {
            var filterLabel = filter switch
            {
                "last_month" => "Último mes",
                "last_year" => "Último año",
                "custom" => $"{from?.ToString("dd/MM/yyyy")} - {to?.ToString("dd/MM/yyyy")}",
                _ => "Todo el tiempo"
            };

            int totalBooks = grouped.SelectMany(g => g).Count(x => bookDict.ContainsKey(x.BookId));

            return theme?.ToLower() switch
            {
                "space" => SpaceLayout.Generate(username, grouped, bookDict, filterLabel, totalBooks),
                "manuscript" => ManuscriptLayout.Generate(username, grouped, bookDict, filterLabel, totalBooks),
                "floral" or _ => FloralLayout.Generate(username, grouped, bookDict, filterLabel, totalBooks)
            };
        }
    }
}
