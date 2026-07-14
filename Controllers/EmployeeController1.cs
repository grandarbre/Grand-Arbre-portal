using Grand_Arbre_portal.Data;
using Grand_Arbre_portal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Grand_Arbre_portal.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public EmployeeController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("UserLogin", "Account");

            // Get employee statistics
            var totalDocuments = await _context.Documents
                .Where(d => d.UploadedByUserId == user.Id && d.IsActive)
                .CountAsync();

            var totalDownloads = await _context.Documents
                .Where(d => d.UploadedByUserId == user.Id && d.IsActive)
                .SumAsync(d => d.DownloadCount);

            var recentDocuments = await _context.Documents
                .Where(d => d.UploadedByUserId == user.Id && d.IsActive)
                .OrderByDescending(d => d.UploadedAt)
                .Take(5)
                .ToListAsync();

            var clients = await _userManager.GetUsersInRoleAsync("Client");
            var activeClients = clients.Where(c => c.IsActive).Count();

            var clientUploads = await _context.Documents
                .Where(d => d.Direction == "ClientToEmployee" && d.IsActive)
                .CountAsync();

            ViewBag.TotalDocuments = totalDocuments;
            ViewBag.TotalDownloads = totalDownloads;
            ViewBag.RecentDocuments = recentDocuments;
            ViewBag.ActiveClients = activeClients;
            ViewBag.ClientUploads = clientUploads;
            ViewBag.UserName = user.FullName;

            return View();
        }

        public async Task<IActionResult> GetEmployeeStats()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { error = "User not found" });

            var totalDocuments = await _context.Documents
                .Where(d => d.UploadedByUserId == user.Id && d.IsActive)
                .CountAsync();

            var totalDownloads = await _context.Documents
                .Where(d => d.UploadedByUserId == user.Id && d.IsActive)
                .SumAsync(d => d.DownloadCount);

            var clients = await _userManager.GetUsersInRoleAsync("Client");
            var activeClients = clients.Where(c => c.IsActive).Count();

            var clientUploads = await _context.Documents
                .Where(d => d.Direction == "ClientToEmployee" && d.IsActive)
                .CountAsync();

            return Json(new
            {
                totalDocuments,
                totalDownloads,
                activeClients,
                clientUploads,
                userName = user.FullName,
                userEmail = user.Email
            });
        }

        public async Task<IActionResult> GetRecentDocuments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { error = "User not found" });

            var recentDocs = await _context.Documents
                .Where(d => d.UploadedByUserId == user.Id && d.IsActive)
                .OrderByDescending(d => d.UploadedAt)
                .Take(10)
                .Select(d => new
                {
                    d.Id,
                    d.Title,
                    d.FileName,
                    d.FilePath,
                    d.UploadedAt,
                    d.DownloadCount,
                    d.ClientUserName,
                    d.Status,
                    UploadedAtFormatted = d.UploadedAt.ToString("MMM dd, yyyy HH:mm")
                })
                .ToListAsync();

            return Json(recentDocs);
        }

        public async Task<IActionResult> GetClientUploads()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { error = "User not found" });

            var clientUploads = await _context.Documents
                .Where(d => d.Direction == "ClientToEmployee" && d.IsActive)
                .OrderByDescending(d => d.UploadedAt)
                .Take(10)
                .Select(d => new
                {
                    d.Id,
                    d.Title,
                    d.FileName,
                    d.FilePath,
                    d.UploadedByUserName,
                    d.UploadedAt,
                    d.DownloadCount,
                    UploadedAtFormatted = d.UploadedAt.ToString("MMM dd, yyyy HH:mm")
                })
                .ToListAsync();

            return Json(clientUploads);
        }
    }
}