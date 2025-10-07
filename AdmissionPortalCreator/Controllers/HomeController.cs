using AdmissionPortalCreator.Data;
using AdmissionPortalCreator.Models;
using AdmissionPortalCreator.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


namespace AdmissionPortalCreator.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Tenant info
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.TenantId == user.TenantId);

            // Users with roles (Admin can see all, Manager maybe restricted)
            var users = await _userManager.Users
                .Where(u => u.TenantId == user.TenantId)
                .ToListAsync();

            var userViewModels = new List<UserViewModel>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                userViewModels.Add(new UserViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    Role = roles.FirstOrDefault() ?? "User"
                });
            }

            // Build Dashboard model
            var dashboard = new DashboardViewModel
            {
                Tenant = tenant,
                Users = userViewModels
            };

            return View(dashboard);
        }


    }
}
