using Application.Dtos;
using Application.ViewModels;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookDiary.Controllers
{
    [Authorize]
    public class BookController : Controller
    {
        private readonly IBookService _service;
        private readonly IOpenLibraryService _openLibraryService;

        public BookController(IBookService bookService, IOpenLibraryService libraryService)
        {
            _service = bookService;
            _openLibraryService = libraryService;
        }


        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string q)
        {

            if (string.IsNullOrWhiteSpace(q) || q.Length < 1 || q == "")
            {
                ViewBag.Query = "";
                return View(new List<BookInfoViewModel>());
            }

            var result = await _service.Search(q);

            var vm = result.Select(x => new BookInfoViewModel
            {
                Title = x.Title,
                Author = x.Author,
                FirstPublishYear = x.FirstPublishYear,
                BookWorkKey = x.BookWorkKey,
                CoverEditionKey = x.CoverEditionKey
            }).ToList();

            ViewBag.Query = q;

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string bookWorkKey, string? q)
        {
            var dto = await _service.GetDetails(q, bookWorkKey);

            if (dto == null)
                return NotFound();

            var vm = new BookDetailsViewModel
            {
                Title = dto.Title,
                Author = dto.Author,
                Year = dto.Year,
                CoverEditionKey = dto.CoverEditionKey,
                AverageRating = dto.AverageRating,
                IsInReading = dto.IsInReading,
                IsCompleted = dto.IsCompleted,
                SelfRating = dto.SelfRating,
                SelfFinished = dto.SelfFinished,
                BookWorkKey = bookWorkKey,
                IntId = dto.IntId,
                IsFavorite = dto.IsFavorite,
                FavoriteSlotIndex = dto.FavoriteSlotIndex
            };

            ViewBag.Query = q;

            return View(vm);
        }
    }
}