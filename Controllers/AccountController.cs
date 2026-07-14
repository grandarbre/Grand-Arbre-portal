using Grand_Arbre_portal.Data;
using Grand_Arbre_portal.Models;
using Grand_Arbre_portal.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Grand_Arbre_portal.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult AdminLogin(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Dashboard", "Admin");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AdminLogin(AdminLoginViewModel model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                    if (isAdmin)
                    {
                        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                        if (result.Succeeded)
                        {
                            return RedirectToAction("Dashboard", "Admin");
                        }
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid admin credentials.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult UserLogin(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]  // <-- Only ONE HttpPost attribute here!
        public async Task<IActionResult> UserLogin(UserLoginViewModel model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or access code.");
                    return View(model);
                }

                if (!user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Your account has been suspended. Please contact your administrator.");
                    return View(model);
                }

                if (user.AccessCode != model.AccessCode)
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or access code.");
                    return View(model);
                }

                await _signInManager.SignInAsync(user, model.RememberMe);
                user.LastLoginAt = DateTime.Now;
                await _userManager.UpdateAsync(user);

                // Redirect based on role
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (await _userManager.IsInRoleAsync(user, "Client"))
                {
                    return RedirectToAction("Dashboard", "Client");
                }
                else if (await _userManager.IsInRoleAsync(user, "Employee"))
                {
                    return RedirectToAction("Index", "Home");
                }

                return RedirectToLocal(returnUrl);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }
    }
}