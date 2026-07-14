using Grand_Arbre_portal.Data;
using Grand_Arbre_portal.Models;
using Grand_Arbre_portal.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;

namespace Grand_Arbre_portal.Controllers
{
    public class AnnouncementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AnnouncementController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        // View all announcements (for clients)
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Index()
        {
            var announcements = await _context.Announcements
                .Where(a => a.IsActive && (a.ExpiryDate == null || a.ExpiryDate > DateTime.Now))
                .OrderByDescending(a => a.IsImportant)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(announcements);
        }

        // Create announcement (for Admin and Employee)
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateAnnouncementViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        TempData["Error"] = "User not found";
                        return View(model);
                    }

                    var announcement = new Announcement
                    {
                        Title = model.Title,
                        Content = model.Content,
                        CreatedByUserId = user.Id,
                        CreatedByUserName = user.FullName,
                        CreatedAt = DateTime.Now,
                        ExpiryDate = model.ExpiryDate,
                        IsActive = true,
                        IsImportant = model.IsImportant
                    };

                    _context.Announcements.Add(announcement);
                    await _context.SaveChangesAsync();

                    // Try to send email notifications
                    try
                    {
                        // Send to all clients
                        await SendEmailToClients(announcement);

                        // Send notification to YOU (the admin)
                        await SendEmailToAdmin(announcement);

                        TempData["Success"] = "Announcement created and emails sent!";
                    }
                    catch (Exception emailEx)
                    {
                        TempData["Success"] = "Announcement created successfully! (Emails could not be sent. Please check email settings.)";
                        Console.WriteLine($"Email error: {emailEx.Message}");
                    }

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error creating announcement: {ex.Message}";
                    return View(model);
                }
            }

            return View(model);
        }

        // Send email to all clients
        private async Task SendEmailToClients(Announcement announcement)
        {
            try
            {
                var clients = await _userManager.GetUsersInRoleAsync("Client");
                var activeClients = clients.Where(c => c.IsActive && !string.IsNullOrEmpty(c.Email)).ToList();

                if (!activeClients.Any())
                {
                    Console.WriteLine("No active clients found to send emails.");
                    return;
                }

                int sentCount = 0;
                int failCount = 0;

                foreach (var client in activeClients)
                {
                    try
                    {
                        await SendEmail(client.Email, client.FullName, announcement);
                        sentCount++;
                        Console.WriteLine($"✅ Email sent to {client.Email}");
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        Console.WriteLine($"❌ Failed to send email to {client.Email}: {ex.Message}");
                    }
                }

                Console.WriteLine($"📊 Client Email Summary: Sent: {sentCount}, Failed: {failCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Email error: {ex.Message}");
                throw;
            }
        }

        // Send notification to Admin (YOU)
        private async Task SendEmailToAdmin(Announcement announcement)
        {
            try
            {
                var adminEmail = _configuration["EmailSettings:NotifyEmail"] ?? "lehlogonolomoshoeu223@gmail.com";

                var subject = $"📢 New Announcement Created: {announcement.Title}";
                var body = $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ background: #0F4C5C; padding: 20px; color: white; text-align: center; }}
                            .header h1 {{ margin: 0; color: #D4AF37; }}
                            .content {{ padding: 20px; background: #f8f9fa; }}
                            .important {{ border-left: 4px solid #dc3545; padding-left: 15px; }}
                            .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                            hr {{ border: none; border-top: 1px solid #ddd; margin: 20px 0; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>🌳 Grand Arbre Consulting</h1>
                                <p style='margin: 0;'>Alive. Inspired. Motivated.</p>
                            </div>
                            <div class='content'>
                                <h2>📢 New Announcement Created!</h2>
                                <hr>
                                <p><strong>Title:</strong> {announcement.Title}</p>
                                <p><strong>Content:</strong> {announcement.Content}</p>
                                <p><strong>Posted by:</strong> {announcement.CreatedByUserName}</p>
                                <p><strong>Date:</strong> {announcement.CreatedAt.ToString("MMMM dd, yyyy HH:mm")}</p>
                                {(announcement.ExpiryDate != null ? $"<p><strong>Expires:</strong> {announcement.ExpiryDate.Value.ToString("MMMM dd, yyyy")}</p>" : "")}
                                <p><strong>Important:</strong> {(announcement.IsImportant ? "⚠️ Yes" : "No")}</p>
                                <hr>
                                <p>This is a notification sent to the administrator.</p>
                            </div>
                            <div class='footer'>
                                <p>&copy; 2017 Grand Arbre Consulting. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>
                ";

                await SendEmail(adminEmail, "Administrator", announcement, subject, body);
                Console.WriteLine($"✅ Admin notification sent to {adminEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send admin notification: {ex.Message}");
            }
        }

        // Main email sending method
        private async Task SendEmail(string toEmail, string toName, Announcement announcement, string? customSubject = null, string? customBody = null)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "lehlogonolomoshoeu223@gmail.com";
                var senderPassword = _configuration["EmailSettings:SenderPassword"] ?? "";
                var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

                if (string.IsNullOrEmpty(senderPassword))
                {
                    Console.WriteLine("⚠️ Email password not configured. Skipping email send.");
                    return;
                }

                string subject;
                string body;

                if (!string.IsNullOrEmpty(customSubject) && !string.IsNullOrEmpty(customBody))
                {
                    subject = customSubject;
                    body = customBody;
                }
                else
                {
                    subject = announcement.IsImportant
                        ? $"🔔 IMPORTANT: {announcement.Title}"
                        : $"📢 New Announcement: {announcement.Title}";

                    body = $@"
                        <html>
                        <head>
                            <style>
                                body {{ font-family: Arial, sans-serif; }}
                                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                                .header {{ background: #0F4C5C; padding: 20px; color: white; text-align: center; }}
                                .header h1 {{ margin: 0; color: #D4AF37; }}
                                .content {{ padding: 20px; background: #f8f9fa; }}
                                .important {{ border-left: 4px solid #dc3545; padding-left: 15px; }}
                                .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                                hr {{ border: none; border-top: 1px solid #ddd; margin: 20px 0; }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>
                                    <h1>🌳 Grand Arbre Consulting</h1>
                                    <p style='margin: 0;'>Alive. Inspired. Motivated.</p>
                                </div>
                                <div class='content'>
                                    <h2>{announcement.Title}</h2>
                                    <div class='{(announcement.IsImportant ? "important" : "")}'>
                                        <span style='background: {(announcement.IsImportant ? "#dc3545" : "#D4AF37")}; color: {(announcement.IsImportant ? "white" : "#0F4C5C")}; padding: 5px 10px; border-radius: 20px; font-size: 12px; font-weight: bold;'>
                                            {(announcement.IsImportant ? "⚠️ Important" : "📢 New")}
                                        </span>
                                    </div>
                                    <p style='white-space: pre-wrap;'>{announcement.Content}</p>
                                    <hr>
                                    <p><strong>Posted by:</strong> {announcement.CreatedByUserName}</p>
                                    <p><strong>Date:</strong> {announcement.CreatedAt.ToString("MMMM dd, yyyy HH:mm")}</p>
                                    {(announcement.ExpiryDate != null ? $"<p><strong>Expires:</strong> {announcement.ExpiryDate.Value.ToString("MMMM dd, yyyy")}</p>" : "")}
                                </div>
                                <div class='footer'>
                                    <p>This is an automated notification from Grand Arbre Consulting.</p>
                                    <p>To view all announcements, please <a href='https://localhost:44389/Account/UserLogin' style='color: #D4AF37;'>login</a> to your account.</p>
                                    <p>&copy; 2017 Grand Arbre Consulting. All rights reserved.</p>
                                </div>
                            </div>
                        </body>
                        </html>
                    ";
                }

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = enableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "Grand Arbre Consulting"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Email error for {toEmail}: {ex.Message}");
                throw;
            }
        }

        // Manage announcements (for Admin and Employee)
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Manage()
        {
            var announcements = await _context.Announcements
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(announcements);
        }

        // Edit announcement
        [Authorize(Roles = "Admin,Employee")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (!isAdmin && announcement.CreatedByUserId != user.Id)
                return Forbid();

            var model = new CreateAnnouncementViewModel
            {
                Title = announcement.Title,
                Content = announcement.Content,
                ExpiryDate = announcement.ExpiryDate,
                IsImportant = announcement.IsImportant
            };

            ViewBag.AnnouncementId = id;
            return View(model);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        public async Task<IActionResult> Edit(int id, CreateAnnouncementViewModel model)
        {
            if (ModelState.IsValid)
            {
                var announcement = await _context.Announcements.FindAsync(id);
                if (announcement == null)
                    return NotFound();

                var user = await _userManager.GetUserAsync(User);
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                if (!isAdmin && announcement.CreatedByUserId != user.Id)
                    return Forbid();

                announcement.Title = model.Title;
                announcement.Content = model.Content;
                announcement.ExpiryDate = model.ExpiryDate;
                announcement.IsImportant = model.IsImportant;
                announcement.IsActive = true;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Announcement updated successfully!";
                return RedirectToAction("Manage");
            }

            ViewBag.AnnouncementId = id;
            return View(model);
        }

        // Delete announcement
        [Authorize(Roles = "Admin,Employee")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var announcement = await _context.Announcements.FindAsync(id);
                if (announcement == null)
                    return Json(new { success = false, message = "Announcement not found" });

                var user = await _userManager.GetUserAsync(User);
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                if (!isAdmin && announcement.CreatedByUserId != user.Id)
                    return Json(new { success = false, message = "You can only delete your own announcements" });

                _context.Announcements.Remove(announcement);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Announcement deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Get announcements (for API)
        public async Task<IActionResult> GetAnnouncements()
        {
            try
            {
                var announcements = await _context.Announcements
                    .Where(a => a.IsActive && (a.ExpiryDate == null || a.ExpiryDate > DateTime.Now))
                    .OrderByDescending(a => a.IsImportant)
                    .ThenByDescending(a => a.CreatedAt)
                    .Select(a => new
                    {
                        a.Id,
                        a.Title,
                        a.Content,
                        a.CreatedByUserName,
                        CreatedAtFormatted = a.CreatedAt.ToString("MMMM dd, yyyy HH:mm"),
                        a.IsImportant,
                        ExpiryDateFormatted = a.ExpiryDate != null ? a.ExpiryDate.Value.ToString("MMMM dd, yyyy") : "Never"
                    })
                    .ToListAsync();

                return Json(announcements);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}