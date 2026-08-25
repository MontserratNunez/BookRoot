using Application.Dtos.User;
using Application.Interfaces;
using Application.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookDiary.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAchievementService _achievementService;
        private readonly IAuthenticationService _authService;

        public UserController(IUserService userService, IAchievementService achievementService, IAuthenticationService authService)
        {
            _userService = userService;
            _achievementService = achievementService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                ViewBag.Query = "";
                return View(new List<UserSearchViewModel>());
            }

            var result = await _userService.SearchUsers(q);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                ViewBag.Query = q;
                return View(new List<UserSearchViewModel>());
            }

            var vm = result.Data!.Select(x => new UserSearchViewModel
            {
                Username = x.Username,
                AvatarUrl = x.AvatarUrl
            }).ToList();

            ViewBag.Query = q;

            return View(vm);
        }

        [HttpGet("User/Profile/{username?}")]
        public async Task<IActionResult> Profile(string? username = null)
        {
            var result = await _userService.GetProfileByUsername(username);

            if (!result.IsSuccess)
            {
                return RedirectToAction("Index", "Home");
            }

            var vm = new UserProfileViewModel
            {
                Username = result.Data.Username,
                Bio = result.Data.Bio,
                AvatarUrl = result.Data.AvatarUrl,
                IsOwner = result.Data.IsOwner,
                Follows = result.Data.Follows,
                Following = result.Data.Following,
                Followers = result.Data.Followers
            };

            return View(vm);
        }



        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var result = await _userService.GetProfileForEdit();

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index", "Home");
            }

            var vm = new EditProfileViewModel
            {
                Username = result.Data!.Username,
                Bio = result.Data.Bio
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var dto = new UpdateProfileDto
            {
                Username = model.Username,
                Bio = model.Bio
            };

            var result = await _userService.UpdateProfile(dto);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            TempData["Success"] = result.Message;
            return RedirectToAction("Edit");
        }

        [HttpGet]
        public async Task<IActionResult> EditProfileImage()
        {
            var profileResult = await _userService.GetProfileForEdit();
            var avatarsResult = await _userService.GetAvailableAvatars();

            if (!profileResult.IsSuccess)
            {
                TempData["Error"] = avatarsResult.Message;
                return RedirectToAction("Index", "Home");
            }

            var vm = new ProfileImageViewModel
            {
                CurrentAvatarUrl = profileResult.Data!.AvatarUrl != null ? profileResult.Data!.AvatarUrl : "/images/profile-images/default-avatar.png",
                AvailableAvatars = avatarsResult.Data!
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfileImage(string selectedAvatar)
        {
            var result = await _userService.UpdateProfileImage(selectedAvatar);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(EditProfileImage));
            }

            TempData["Success"] = result.Message;
            return RedirectToAction(nameof(EditProfileImage));
        }

        [HttpGet("User/GetCompletedBooksAsync")]
        public async Task<IActionResult> GetCompletedBooksAsync(string username)
        {
            var result = await _userService.GetCompletedBooks(username);

            if (!result.IsSuccess)
            {
                return BadRequest($"Error: {result.Message}");
            }

            if (result.Data == null)
            {
                return Content($"<p class='text-muted'>{result.Message}</p>");
            }

            var vm = result.Data != null ? result.Data.Select(x => new UserCompletedBookViewModel
            {
                BookKey = x.BookWorkKey,
                Title = x.Title,
                Author = x.Author
            }).ToList() : new List<UserCompletedBookViewModel>();

            return PartialView("_CompletedBooksList", vm);
        }

        [HttpGet]
        public async Task<IActionResult> EditTopFour()
        {
            var result = await _userService.GetTopFour(null);

            if (!result.IsSuccess) return NotFound();

            var currentTop = result.Data ?? new List<TopFourBookItemDto>();

            var vm = new TopFourViewModel
            {
                BookKey1 = currentTop.Count > 0 ? currentTop[0].BookWorkKey : "",
                BookCover1 = currentTop.Count > 0 ? currentTop[0].CoverUrl : "",
                BookTitle1 = currentTop.Count > 0 ? currentTop[0].Title : "",

                BookKey2 = currentTop.Count > 1 ? currentTop[1].BookWorkKey : "",
                BookCover2 = currentTop.Count > 1 ? currentTop[1].CoverUrl : "",
                BookTitle2 = currentTop.Count > 1 ? currentTop[1].Title : "",

                BookKey3 = currentTop.Count > 2 ? currentTop[2].BookWorkKey : "",
                BookCover3 = currentTop.Count > 2 ? currentTop[2].CoverUrl : "",
                BookTitle3 = currentTop.Count > 2 ? currentTop[2].Title : "",

                BookKey4 = currentTop.Count > 3 ? currentTop[3].BookWorkKey : "",
                BookCover4 = currentTop.Count > 3 ? currentTop[3].CoverUrl : "",
                BookTitle4 = currentTop.Count > 3 ? currentTop[3].Title : "",
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetTopFourJson(string? username = null)
        {
            var result = await _userService.GetTopFour(username);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message });
            }

            return Json(result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTopFour(TopFourViewModel model)
        {
            var dto = new UpdateTopFourDto
            {
                BookKeys = new List<string>
        {
            model.BookKey1!,
            model.BookKey2!,
            model.BookKey3!,
            model.BookKey4!
        }
            };

            var result = await _userService.UpdateTopFour(dto);

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return View(model);
            }

            TempData["Success"] = result.Message;

            var username = User.Identity?.Name;
            return RedirectToAction("Profile", new { username });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToFavorites(string bookWorkKey, int slotIndex, string? returnBookKey)
        {
            var result = await _userService.AddBookToTopFour(bookWorkKey, slotIndex);

            if (!result.IsSuccess)
                TempData["Error"] = result.Message;
            else
                TempData["Success"] = result.Message;

            if (!string.IsNullOrEmpty(returnBookKey))
                return RedirectToAction("Details", "Book", new { bookWorkKey = returnBookKey });

            return RedirectToAction("Profile", new { username = User.Identity?.Name });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromFavorites(string bookWorkKey, string? returnBookKey)
        {
            var result = await _userService.RemoveBookFromTopFour(bookWorkKey);

            if (!result.IsSuccess)
                TempData["Error"] = result.Message;
            else
                TempData["Success"] = result.Message;

            if (!string.IsNullOrEmpty(returnBookKey))
                return RedirectToAction("Details", "Book", new { bookWorkKey = returnBookKey });

            return RedirectToAction("Profile", new { username = User.Identity?.Name });
        }

        public IActionResult Config()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(Application.ViewModels.Auth.ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authService.UpdatePassword(model.NewPassword);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                return View(model);
            }

            TempData["Success"] = "Contraseña actualizada exitosamente.";
            return RedirectToAction("Config");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var result = await _userService.DeleteAccount();

            if (!result.IsSuccess)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Config");
            }

            Response.Cookies.Delete("AuthToken");
            Response.Cookies.Delete("SupabaseRefreshToken");

            return RedirectToAction("Index", "Public");
        }

        [HttpGet("User/GetAchievementsJson")]
        public async Task<IActionResult> GetAchievementsJson(string username)
        {
            var profileResult = await _userService.GetProfileByUsername(username);
            if (!profileResult.IsSuccess)
                return Json(new List<object>());

            var profileId = profileResult.Data!.Id;
            var achievementsResult = await _achievementService.GetUserAchievements(profileId);
            
            if (!achievementsResult.IsSuccess)
                return Json(new List<object>());

            return Json(achievementsResult.Data);
        }
    }
}
