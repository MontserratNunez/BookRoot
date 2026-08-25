using Application.Common.Result;
using Application.Dtos.Book;
using Application.Interfaces;
using Application.ViewModels.Book;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookDiary.Controllers
{

    [Authorize]
    public class InteractionController : Controller
    {
        private readonly IInteractionService _service;

        public InteractionController(IInteractionService service)
        {
            _service = service;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CreateInteractionViewModel vm)
        {
            var dto = new CreateInteractionDto
            {
                BookWorkKey = vm.BookWorkKey,
                Status = vm.Status,
                Query = vm.Query,
                FinishedAt = vm.FinishedAt,
                Rating = vm.Rating
            };

            var result = await _service.AddOrUpdate(dto);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
            }

            return RedirectToAction("Details", "Book", new { bookWorkKey = dto.BookWorkKey});
        }

        [HttpGet("Books")]
        public async Task<IActionResult> Lists()
        {
            var result = await _service.GetUserLists();

            if (result == null || result.Data == null)
            {
                return View(new UserListsViewModel
                {
                    Reading = new List<ReadingViewModel>(),
                    Completed = new List<CompletedViewModel>()
                });

            }

            var vm = new UserListsViewModel
            {
                Reading = result.Data.Reading != null ? result.Data.Reading.Select(x => new ReadingViewModel
                {
                    Id = x.Id,
                    BookKey = x.BookWorkKey,
                    Title = x.Title,
                    Author = x.Author
                }).ToList() : new List<ReadingViewModel>(),

                Completed = result.Data.Completed != null ? result.Data.Completed.Select(x => new CompletedViewModel
                {
                    Id = x.Id,
                    BookKey = x.BookWorkKey,
                    Title = x.Title,
                    Author = x.Author,
                    FinishedAt = x.FinishedAt,
                    Rating = x.Rating
                }).ToList() : new List<CompletedViewModel>()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReaded(EditReadedViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Lists));
            }

            var dto = new EditReadedBookDto
            {
                Id = vm.Id,
                Date = vm.Date,
                Rating = vm.Rating
            };

            var result = await _service.EditReaded(dto);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Lists));
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Lists));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsReaded(string id)
        {
            var result = await _service.MarkAsReaded(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Lists));
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(Lists));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id, string? bookKey)
        {
            var result = await _service.Delete(id);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = result.Message;
            }

            if(bookKey != null)
            {
                return RedirectToAction("Details", "Book", new { bookWorkKey = bookKey });
            }
            return RedirectToAction(nameof(Lists));
        }
    }
}
