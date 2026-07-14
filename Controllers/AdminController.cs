using Grand_Arbre_portal.Data;
using Grand_Arbre_portal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Grand_Arbre_portal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult ManageUsers()
        {
            return View();
        }

        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userList = new List<object>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    userList.Add(new
                    {
                        user.Id,
                        user.FullName,
                        user.Email,
                        user.IsActive,
                        user.CreatedAt,
                        user.AccessCode,
                        Role = roles.FirstOrDefault() ?? "None"
                    });
                }

                return Json(userList);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUser(string fullName, string email, string role)
        {
            try
            {
                if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email))
                    return Json(new { success = false, message = "Name and email are required" });

                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                    return Json(new { success = false, message = "Email already exists" });

                // Generate unique access code
                string prefix = role == "Employee" ? "EMP" : "CLT";
                string accessCode = prefix + DateTime.Now.ToString("yyyyMMdd") + new Random().Next(100, 999).ToString();

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                    AccessCode = accessCode,
                    IsFirstLogin = true
                };

                var tempPassword = "Temp@123" + new Random().Next(100, 999).ToString();
                var result = await _userManager.CreateAsync(user, tempPassword);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, role);
                    return Json(new { success = true, message = "User added successfully!", accessCode = accessCode });
                }

                return Json(new { success = false, message = "Failed to add user" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateAccessCode(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                var roles = await _userManager.GetRolesAsync(user);
                string role = roles.FirstOrDefault() ?? "Client";
                string prefix = role == "Employee" ? "EMP" : "CLT";
                string newCode = prefix + DateTime.Now.ToString("yyyyMMdd") + new Random().Next(100, 999).ToString();

                user.AccessCode = newCode;
                await _userManager.UpdateAsync(user);

                return Json(new { success = true, message = "New access code generated!", code = newCode });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null && currentUser.Id == userId)
                    return Json(new { success = false, message = "Cannot change your own status" });

                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);

                string status = user.IsActive ? "activated" : "suspended";
                return Json(new { success = true, message = $"User {status}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Json(new { success = false, message = "User not found" });

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null && currentUser.Id == userId)
                    return Json(new { success = false, message = "Cannot delete your own account" });

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                    return Json(new { success = true, message = "User deleted" });

                return Json(new { success = false, message = "Delete failed" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}