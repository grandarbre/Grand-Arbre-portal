using Grand_Arbre_portal.Data;
using Grand_Arbre_portal.Models;
using Grand_Arbre_portal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Grand_Arbre_portal.Controllers
{
    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocumentController(
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _environment = environment;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet]
        public async Task<IActionResult> Upload()
        {
            var user = await _userManager.GetUserAsync(User);
            var clients = await _userManager.GetUsersInRoleAsync("Client");
            ViewBag.Clients = clients.Where(c => c.IsActive).ToList();
            return View();
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        public async Task<IActionResult> Upload(UploadDocumentViewModel model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                var client = await _userManager.FindByIdAsync(model.SelectedClientId);
                if (client == null)
                    return Json(new { success = false, message = "Client not found" });

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.File.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }

                var document = new Document
                {
                    Title = model.Title,
                    Description = model.Description ?? "",
                    FileName = model.File.FileName,
                    FilePath = "/uploads/" + uniqueFileName,
                    FileType = model.File.ContentType,
                    FileSize = model.File.Length,
                    UploadedByUserId = user.Id,
                    UploadedByUserName = user.FullName,
                    ClientUserId = client.Id,
                    ClientUserName = client.FullName,
                    Direction = "EmployeeToClient",
                    Status = "Active",
                    UploadedAt = DateTime.Now,
                    IsActive = true,
                    ExpiryDate = model.ExpiryDate,
                    Category = model.Category ?? "Other"
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Document uploaded successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Client upload document - GET
        [Authorize(Roles = "Client")]
        [HttpGet]
        public IActionResult ClientUpload()
        {
            return View();
        }

        // Client upload document - POST
        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> ClientUpload(ClientUploadViewModel model)
        {
            try
            {
                var client = await _userManager.GetUserAsync(User);
                if (client == null)
                    return Json(new { success = false, message = "User not found" });

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.File.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }

                var document = new Document
                {
                    Title = model.Title,
                    Description = model.Description ?? "",
                    FileName = model.File.FileName,
                    FilePath = "/uploads/" + uniqueFileName,
                    FileType = model.File.ContentType,
                    FileSize = model.File.Length,
                    UploadedByUserId = client.Id,
                    UploadedByUserName = client.FullName,
                    ClientUserId = client.Id,
                    ClientUserName = client.FullName,
                    Direction = "ClientToEmployee",
                    Status = "Active",
                    UploadedAt = DateTime.Now,
                    IsActive = true,
                    ExpiryDate = model.ExpiryDate,
                    Category = model.Category ?? "Other"
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Document uploaded successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Client upload for employee - GET (Legacy - keep for compatibility)
        [Authorize(Roles = "Client")]
        [HttpGet]
        public IActionResult UploadForEmployee()
        {
            return View();
        }

        // Client upload for employee - POST (Legacy - keep for compatibility)
        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> UploadForEmployee(ClientUploadViewModel model)
        {
            try
            {
                var client = await _userManager.GetUserAsync(User);
                if (client == null)
                    return Json(new { success = false, message = "User not found" });

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.File.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(stream);
                }

                var document = new Document
                {
                    Title = model.Title,
                    Description = model.Description ?? "",
                    FileName = model.File.FileName,
                    FilePath = "/uploads/" + uniqueFileName,
                    FileType = model.File.ContentType,
                    FileSize = model.File.Length,
                    UploadedByUserId = client.Id,
                    UploadedByUserName = client.FullName,
                    ClientUserId = client.Id,
                    ClientUserName = client.FullName,
                    Direction = "ClientToEmployee",
                    Status = "Active",
                    UploadedAt = DateTime.Now,
                    IsActive = true,
                    ExpiryDate = model.ExpiryDate,
                    Category = model.Category ?? "Other"
                };

                _context.Documents.Add(document);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Document uploaded successfully for employee review!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize]
        public async Task<IActionResult> MyDocuments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isEmployee = await _userManager.IsInRoleAsync(user, "Employee");
            var isClient = await _userManager.IsInRoleAsync(user, "Client");

            List<Document> documents = new List<Document>();

            if (isAdmin)
            {
                documents = await _context.Documents
                    .Where(d => d.IsActive)
                    .OrderByDescending(d => d.UploadedAt)
                    .ToListAsync();
            }
            else if (isEmployee)
            {
                documents = await _context.Documents
                    .Where(d => d.IsActive && (d.UploadedByUserId == user.Id || d.Direction == "ClientToEmployee"))
                    .OrderByDescending(d => d.UploadedAt)
                    .ToListAsync();
            }
            else if (isClient)
            {
                documents = await _context.Documents
                    .Where(d => d.IsActive && d.ClientUserId == user.Id)
                    .OrderByDescending(d => d.UploadedAt)
                    .ToListAsync();
            }

            return View(documents);
        }

        [Authorize(Roles = "Admin,Employee,Client")]
        public async Task<IActionResult> Download(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var doc = await _context.Documents.FindAsync(id);
            if (doc == null)
                return NotFound();

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isEmployee = await _userManager.IsInRoleAsync(user, "Employee");
            var isClient = await _userManager.IsInRoleAsync(user, "Client");

            bool canDownload = false;

            if (isAdmin)
            {
                canDownload = true;
            }
            else if (isEmployee)
            {
                if (doc.UploadedByUserId == user.Id || doc.Direction == "ClientToEmployee")
                {
                    canDownload = true;
                }
            }
            else if (isClient)
            {
                if (doc.ClientUserId == user.Id)
                {
                    canDownload = true;
                }
            }

            if (!canDownload)
            {
                return Forbid();
            }

            doc.DownloadCount++;
            await _context.SaveChangesAsync();

            var filePath = Path.Combine(_environment.WebRootPath, doc.FilePath.TrimStart('/'));
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(fileBytes, doc.FileType, doc.FileName);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "User not found" });

            var doc = await _context.Documents.FindAsync(id);
            if (doc == null)
                return Json(new { success = false, message = "Document not found" });

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (!isAdmin && doc.UploadedByUserId != user.Id)
            {
                return Json(new { success = false, message = "You can only delete your own documents" });
            }

            doc.IsActive = false;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Document deleted successfully" });
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        public async Task<IActionResult> ReplaceDocument(int id, IFormFile newFile, string title, string description)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                var existingDoc = await _context.Documents.FindAsync(id);
                if (existingDoc == null)
                    return Json(new { success = false, message = "Document not found" });

                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                if (!isAdmin && existingDoc.UploadedByUserId != user.Id)
                {
                    return Json(new { success = false, message = "You can only replace your own documents" });
                }

                var oldFilePath = Path.Combine(_environment.WebRootPath, existingDoc.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                    System.IO.File.Delete(oldFilePath);

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + newFile.FileName;
                var newFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(newFilePath, FileMode.Create))
                {
                    await newFile.CopyToAsync(stream);
                }

                existingDoc.Title = title;
                existingDoc.Description = description ?? "";
                existingDoc.FileName = newFile.FileName;
                existingDoc.FilePath = "/uploads/" + uniqueFileName;
                existingDoc.FileType = newFile.ContentType;
                existingDoc.FileSize = newFile.Length;
                existingDoc.UploadedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Document replaced successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin,Employee,Client")]
        public async Task<IActionResult> GetDocuments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "User not found" });

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isEmployee = await _userManager.IsInRoleAsync(user, "Employee");
            var isClient = await _userManager.IsInRoleAsync(user, "Client");

            IQueryable<Document> query = _context.Documents.Where(d => d.IsActive);

            if (isAdmin)
            {
                // Admin sees all
            }
            else if (isEmployee)
            {
                query = query.Where(d => d.UploadedByUserId == user.Id || d.Direction == "ClientToEmployee");
            }
            else if (isClient)
            {
                query = query.Where(d => d.ClientUserId == user.Id);
            }
            else
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            var documents = await query
                .OrderByDescending(d => d.UploadedAt)
                .Select(d => new
                {
                    d.Id,
                    d.Title,
                    d.Description,
                    d.FileName,
                    d.FilePath,
                    d.FileType,
                    d.UploadedByUserName,
                    d.ClientUserName,
                    d.Direction,
                    UploadedAtFormatted = d.UploadedAt.ToString("yyyy-MM-dd HH:mm"),
                    d.DownloadCount,
                    d.ExpiryDate,
                    d.Category,  // ADDED: Include Category
                    CanDelete = isAdmin || d.UploadedByUserId == user.Id
                })
                .ToListAsync();

            return Json(documents);
        }

        // Document Dashboard
        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("UserLogin", "Account");

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isEmployee = await _userManager.IsInRoleAsync(user, "Employee");
            var isClient = await _userManager.IsInRoleAsync(user, "Client");

            List<Document> documents = new List<Document>();

            if (isAdmin)
            {
                documents = await _context.Documents
                    .Where(d => d.IsActive)
                    .OrderByDescending(d => d.UploadedAt)
                    .ToListAsync();
            }
            else if (isEmployee)
            {
                documents = await _context.Documents
                    .Where(d => d.IsActive && d.UploadedByUserId == user.Id)
                    .OrderByDescending(d => d.UploadedAt)
                    .ToListAsync();
            }
            else if (isClient)
            {
                documents = await _context.Documents
                    .Where(d => d.IsActive && d.ClientUserId == user.Id)
                    .OrderByDescending(d => d.UploadedAt)
                    .ToListAsync();
            }

            return View(documents);
        }
    }
}