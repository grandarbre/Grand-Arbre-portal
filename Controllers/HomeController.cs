using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Grand_Arbre_portal.Models;

namespace Grand_Arbre_portal.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // If user is already logged in, redirect to their dashboard
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return View();

                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("Dashboard", "Admin");
                else if (await _userManager.IsInRoleAsync(user, "Employee"))
                    return RedirectToAction("Dashboard", "Employee");
                else if (await _userManager.IsInRoleAsync(user, "Client"))
                    return RedirectToAction("Dashboard", "Client");
            }

            // Show landing page for non-authenticated users
            return View();
        }
    }
}