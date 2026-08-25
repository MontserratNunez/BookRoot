using Application.Dtos.Lists;
using Application.Interfaces;
using Application.ViewModels.Lists;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookDiary.Controllers
{
    [Authorize]
    public class ListController : Controller
    {
        private readonly IListService _listService;
        private readonly IBookService _bookService;
        private readonly IOpenLibraryService _openLibraryService;

        public ListController(
            IListService listService,
            IBookService bookService,
            IOpenLibraryService openLibraryService)
        {
            _listService = listService;
            _bookService = bookService;
            _openLibraryService = openLibraryService;
        }

        [HttpGet("Lists/{username?}")]
        public async Task<IActionResult> Index(string? username = null)
        {
            if (string.IsNullOrEmpty(username))
            {
                username = User.Identity?.Name;
            }

            var listsResult = await _listService.GetUserLists(username);


            if (!listsResult.IsSuccess)
            {
                TempData["Error"] = listsResult.Message;
                return RedirectToAction("Index", "Home");
            }

            var vm = new ListIndexViewModel
            {
                Username = username,
                IsOwner = User.Identity?.Name == username,
                Lists = (listsResult.Data ?? new())
                    .Select(l => new ListSummaryViewModel
                    {
                        Id = l.Id,
                        ListName = l.ListName,
                        ListDescription = l.ListDescription,
                        BookCount = l.BooksIds.Count,
                        CreatedAt = l.CreatedAt
                    }).ToList()
            };

            return View(vm);
        }

        [HttpGet("Lists/Details/{listId}")]
        public async Task<IActionResult> Details(string listId)
        {
            var listResult = await _listService.GetListById(listId, null);
            if (!listResult.IsSuccess)
            {
                TempData["Error"] = listResult.Message;
                return RedirectToAction("Index", "Home");
            }

            var listDto = listResult.Data!;

            var books = new List<ListBookItemViewModel>();
            foreach (var key in listDto.BooksIds)
            {
                var details = await _bookService.GetDetails(null, key);
                if (details == null) continue;

                books.Add(new ListBookItemViewModel
                {
                    BookWorkKey = key,
                    Title = details.Title,
                    Author = details.Author ?? "",
                    CoverUrl = details.CoverEditionKey
                });
            }

            var vm = new ListDetailsViewModel
            {
                ListId = listId,
                ListName = listDto.ListName,
                ListDescription = listDto.ListDescription,
                OwnerUsername = listResult.Data.ListOwnerUsername,
                IsOwner = listResult.Data.ListOwnerUsername == User.Identity?.Name,
                Books = books
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateListViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateListViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var dto = new CreateListDto
            {
                ListName = vm.ListName,
                ListDescription = vm.ListDescription
            };

            var result = await _listService.CreateList(dto);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                return View(vm);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction("Index", new { username = User.Identity!.Name });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string listId)
        {
            var result = await _listService.DeleteList(listId);
            if (!result.IsSuccess)
                TempData["Error"] = result.Message;
            else
                TempData["Success"] = result.Message;

            return RedirectToAction("Index", new { username = User.Identity!.Name });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBook(string listId, string bookWorkKey, string? returnBookKey)
        {
            var dto = new AddBookToListDto
            {
                ListId = listId,
                BookWorkKey = bookWorkKey
            };

            var result = await _listService.AddBookToList(dto);

            if (!result.IsSuccess)
                TempData["Error"] = result.Message;
            else
                TempData["Success"] = result.Message;

            if (!string.IsNullOrEmpty(returnBookKey))
                return RedirectToAction("Details", "Book", new { bookWorkKey = returnBookKey });

            return RedirectToAction("Details", new { listId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveBook(string listId, string bookWorkKey, string? returnBookKey = null)
        {
            var result = await _listService.RemoveBookFromList(listId, bookWorkKey);

            if (!result.IsSuccess)
                TempData["Error"] = result.Message;
            else
                TempData["Success"] = result.Message;

            if (!string.IsNullOrEmpty(returnBookKey))
                return RedirectToAction("Details", "Book", new { bookWorkKey = returnBookKey });

            return RedirectToAction("Details", new { listId });
        }

        [HttpGet]
        public async Task<IActionResult> GetMyListsJson(string? bookWorkKey = null)
        {
            var result = await _listService.GetMyLists(User.Identity?.Name);
            if (!result.IsSuccess)
                return Json(new List<object>());

            return Json(result.Data!.Select(l => new 
            { 
                l.Id, 
                l.ListName,
                hasBook = !string.IsNullOrEmpty(bookWorkKey) && l.BooksIds.Contains(bookWorkKey)
            }));
        }
    }
}
