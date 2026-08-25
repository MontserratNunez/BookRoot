using Application.Dtos.Journal;
using Application.Dtos.User;
using Application.Interfaces;
using Application.ViewModels.Journal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BookDiary.Controllers
{
    [Authorize]
    public class JournalController : Controller
    {
        private readonly IJournalService _journalService;
        private readonly IExportService _exportService;
        private readonly IUserService _userService;

        public JournalController(IJournalService journalService, IExportService exportService, IUserService userService)
        {
            _journalService = journalService;
            _exportService = exportService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _journalService.GetUserJournal();

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            var dtos = result.Data ?? new List<JournalItemDto>();

            var viewModel = new JournalViewModel
            {
                Username = User.Identity?.Name,
                Years = dtos
                    .GroupBy(x => x.FinishedAt.Year)
                    .OrderByDescending(gYear => gYear.Key)
                    .Select(gYear => new JournalYearGroupViewModel
                    {
                        Year = gYear.Key,
                        Months = gYear
                            .GroupBy(x => x.FinishedAt.Month)
                            .OrderByDescending(gMonth => gMonth.Key)
                            .Select(gMonth => new JournalMonthGroupViewModel
                            {
                                MonthNumber = gMonth.Key,
                                MonthName = CultureInfo.GetCultureInfo("es-ES").DateTimeFormat.GetMonthName(gMonth.Key).ToUpper(),
                                Books = gMonth.Select(b => new JournalBookViewModel
                                {
                                    BookWorkKey = b.BookWorkKey,
                                    Title = b.Title,
                                    Author = b.Author,
                                    CoverUrl = b.CoverUrl,
                                    Rating = b.Rating,
                                    FinishedDay = b.FinishedAt.ToString("dd")
                                }).ToList()
                            }).ToList()
                    }).ToList()
            };

            return View(viewModel);
        }

        [HttpGet("Journal/Export")]
        public async Task<IActionResult> ExportDiary(string filter = "all", string theme = "floral", DateTime? from = null, DateTime? to = null)
        {
            var result = await _exportService.GenerateHtmlExport(User.Identity?.Name, filter, theme, from, to);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index");
            }

            var html = result.Data;
            var fileName = $"BookRoot_Diary_{theme}_{DateTime.Now:yyyyMMdd}.html";

            var bytes = System.Text.Encoding.UTF8.GetBytes(html!);
            return File(bytes, "text/html", fileName);
        }
    }
}