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
            // Fetch currently signed-in user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Tenant info
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.TenantId == currentUser.TenantId);

            // Get all forms for this tenant
            var forms = await _context.Forms
                .Where(f => f.TenantId == currentUser.TenantId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            // Get users in tenant
            var usersInTenant = await _userManager.Users
                .Where(u => u.TenantId == currentUser.TenantId)
                .ToListAsync();

            var userViewModels = new List<UserViewModel>();

            foreach (var user in usersInTenant)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault();

                // Only add administrative users (exclude Student role)
                if (role != null && role != "Student")
                {
                    userViewModels.Add(new UserViewModel
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName,
                        Role = role
                    });
                }
            }

            // Build Dashboard model
            var dashboard = new DashboardViewModel
            {
                Tenant = tenant,
                Users = userViewModels,
                Forms = forms
            };

            return View(dashboard);
        }

    }
}