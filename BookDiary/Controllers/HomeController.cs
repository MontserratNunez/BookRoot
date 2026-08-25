using Application.Interfaces;
using Application.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookDiary.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _homeService.GetHomeDashboardData();

            var viewModel = new HomeViewModel();

            if (result.IsSuccess && result.Data != null)
            {
                viewModel.MostReadBooks = result.Data.MostReadBooks.Select(b => new HomeBookViewModel
                {
                    BookWorkKey = b.BookWorkKey,
                    Title = b.Title,
                    Author = b.Author,
                    CoverUrl = b.CoverUrl,
                    DisplayRating = b.AverageRating.HasValue ? string.Concat(Enumerable.Repeat("★", (int)b.AverageRating.Value)) : "Sin calificación"
                }).ToList();

                viewModel.FriendsActivity = result.Data.FriendsActivity.Select(a => new HomeFriendActivityViewModel
                {
                    FriendUsername = a.FriendUsername,
                    FriendProfilePicture = !string.IsNullOrEmpty(a.FriendProfilePicture)
                        ? a.FriendProfilePicture
                        : "/images/profile-images/default-avatar.png",
                    BookWorkKey = a.BookWorkKey,
                    BookTitle = a.BookTitle,
                    StarsRating = a.Rating.HasValue
                        ? string.Concat(Enumerable.Repeat("★", a.Rating.Value))
                        : "sin estrellas",
                    TimeAgo = CalcularTiempoRelativo(a.FinishedAt)
                }).ToList();
            }
            else
            {
                TempData["Error"] = result.Message;
            }

                return View(viewModel);
        }

        private static string CalcularTiempoRelativo(DateTime date)
        {
            var ts = DateTime.Now - date;
            if (ts.TotalDays >= 1) return $"hace {(int)ts.TotalDays} día(s)";
            if (ts.TotalHours >= 1) return $"hace {(int)ts.TotalHours} hora(s)";
            return "hace unos momentos";
        }
    }

}

