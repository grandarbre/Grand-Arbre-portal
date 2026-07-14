using Grand_Arbre_portal.Data;
using Grand_Arbre_portal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Grand_Arbre_portal.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ClientController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("UserLogin", "Account");

            ViewBag.UserName = user.FullName;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetClientInfo()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { error = "User not found" });

            return Json(new
            {
                user.FullName,
                user.Email,
                user.AccessCode,
                user.CreatedAt,
                user.IsActive
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetClientStats()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { error = "User not found" });

            var totalDocuments = await _context.Documents
                .Where(d => d.ClientUserId == user.Id && d.IsActive)
                .CountAsync();

            var totalDownloads = await _context.Documents
                .Where(d => d.ClientUserId == user.Id && d.IsActive)
                .SumAsync(d => d.DownloadCount);

            var recentDocuments = await _context.Documents
                .Where(d => d.ClientUserId == user.Id && d.IsActive)
                .CountAsync();

            return Json(new
            {
                totalDocuments,
                totalDownloads,
                recentDocuments
            });
        }
    }
}