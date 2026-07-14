using Grand_Arbre_portal.Data;
using Grand_Arbre_portal.Models;
using Grand_Arbre_portal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Grand_Arbre_portal.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("UserLogin", "Account");

            var roles = await _userManager.GetRolesAsync(user);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var documentCount = await _context.Documents
                .Where(d => d.UploadedByUserId == user.Id && d.IsActive)
                .CountAsync();

            var downloadCount = await _context.Documents
                .Where(d => d.UploadedByUserId == user.Id && d.IsActive)
                .SumAsync(d => d.DownloadCount);

            ViewBag.Role = roles.FirstOrDefault() ?? "None";
            ViewBag.DocumentCount = documentCount;
            ViewBag.DownloadCount = downloadCount;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.ProfilePicturePath = user.ProfilePicturePath;

            return View(user);
        }

        // Upload profile picture
        [HttpPost]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
        {
            if (profilePicture == null || profilePicture.Length == 0)
                return Json(new { success = false, message = "Please select a valid image file." });

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg", "image/gif" };
            if (!allowedTypes.Contains(profilePicture.ContentType))
                return Json(new { success = false, message = "Only JPG, PNG, GIF images are allowed." });

            // Validate file size (max 2MB)
            if (profilePicture.Length > 2 * 1024 * 1024)
                return Json(new { success = false, message = "Image size must be less than 2MB." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "User not found." });

            // Delete old profile picture if exists
            if (!string.IsNullOrEmpty(user.ProfilePicturePath))
            {
                var oldPath = Path.Combine(_environment.WebRootPath, user.ProfilePicturePath.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            // Create uploads/profiles folder if not exists
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Generate unique filename
            var fileName = Guid.NewGuid().ToString() + "_" + profilePicture.FileName;
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(stream);
            }

            // Save path to user
            user.ProfilePicturePath = "/uploads/profiles/" + fileName;
            await _userManager.UpdateAsync(user);

            return Json(new { success = true, message = "Profile picture uploaded successfully!", path = user.ProfilePicturePath });
        }

        // Update profile
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string fullName, string phoneNumber)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "User not found" });

            user.FullName = fullName;
            user.PhoneNumber = phoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return Json(new { success = true, message = "Profile updated successfully!" });

            return Json(new { success = false, message = "Failed to update profile" });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var documentCount = await _context.Documents
                    .Where(d => d.UploadedByUserId == user.Id && d.IsActive)
                    .CountAsync();

                userList.Add(new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.AccessCode,
                    user.IsActive,
                    user.CreatedAt,
                    user.LastLoginAt,
                    user.ProfilePicturePath,
                    Role = roles.FirstOrDefault() ?? "None",
                    DocumentCount = documentCount
                });
            }

            return View(userList);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ViewUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var documentCount = await _context.Documents
                .Where(d => d.UploadedByUserId == user.Id && d.IsActive)
                .CountAsync();

            var downloadCount = await _context.Documents
                .Where(d => d.UploadedByUserId == user.Id && d.IsActive)
                .SumAsync(d => d.DownloadCount);

            ViewBag.Role = roles.FirstOrDefault() ?? "None";
            ViewBag.DocumentCount = documentCount;
            ViewBag.DownloadCount = downloadCount;
            ViewBag.ProfilePicturePath = user.ProfilePicturePath;

            return View(user);
        }
    }
}