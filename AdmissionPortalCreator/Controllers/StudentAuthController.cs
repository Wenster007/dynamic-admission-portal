using AdmissionPortalCreator.Data;
using AdmissionPortalCreator.Models;
using AdmissionPortalCreator.ViewModel;
using AdmissionPortalCreator.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AdmissionPortalCreator.Controllers
{
    public class StudentAuthController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public StudentAuthController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // --------------------------------------------------------
        // Step 1: Entry point  /apply/{tenantId}/{formCode}
        // --------------------------------------------------------
        [HttpGet("/apply/{tenantId:int}/{formCode}")]
        public async Task<IActionResult> Apply(int tenantId, string formCode)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.TenantId == tenantId);
            if (tenant == null)
                return NotFound("Tenant not found.");

            var model = new StudentAccountViewModel
            {
                TenantId = tenantId,
                FormCode = formCode,
                TenantName = tenant.Name,
                IsLoginMode = true // Default = Login
            };

            return View("TenantLoginRegister", model);
        }

        // --------------------------------------------------------
        // Step 2: Toggle Login/Register screens
        // --------------------------------------------------------
        [HttpGet("/apply/{tenantId:int}/{formCode}/register")]
        public async Task<IActionResult> RegisterScreen(int tenantId, string formCode)
        {
            var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.TenantId == tenantId);
            if (tenant == null)
                return NotFound("Tenant not found.");

            var model = new StudentAccountViewModel
            {
                TenantId = tenantId,
                FormCode = formCode,
                TenantName = tenant.Name,
                IsLoginMode = false // Show Register UI
            };

            return View("TenantLoginRegister", model);
        }

        // --------------------------------------------------------
        // Step 3: Register (POST)
        // --------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Register(StudentAccountViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid input." });

            var tenant = await _context.Tenants.FindAsync(model.TenantId);
            if (tenant == null)
                return Json(new { success = false, message = "Tenant not found." });

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return Json(new { success = false, message = "Email already registered." });

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                TenantId = model.TenantId,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync("Student"))
                    await _roleManager.CreateAsync(new IdentityRole("Student"));

                await _userManager.AddToRoleAsync(user, "Student");
                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(user, isPersistent: false);

                HttpContext.Session.SetInt32("TenantId", model.TenantId);
                HttpContext.Session.SetString("FormCode", model.FormCode ?? "");


                return Json(new
                {
                    success = true,
                    message = "Registration successful!",
                    redirectUrl = Url.Action("Apply", new { tenantId = model.TenantId, formCode = model.FormCode })
                });
            }

            return Json(new { success = false, message = string.Join("; ", result.Errors.Select(e => e.Description)) });
        }

        // --------------------------------------------------------
        // Step 4: Login (POST)
        // --------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Login(StudentAccountViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid input." });

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.TenantId == model.TenantId);

            if (user == null)
                return Json(new { success = false, message = "Invalid email or tenant." });

            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);

            if (result.Succeeded)
            {
                // ✅ Store tenant info for redirection after logout
                HttpContext.Session.SetInt32("TenantId", model.TenantId);
                HttpContext.Session.SetString("FormCode", model.FormCode ?? "");

                return Json(new
                {
                    success = true,
                    message = "Login successful!",
                    redirectUrl = Url.Action("Dashboard", "StudentDashboard")
                });
            }

            return Json(new { success = false, message = "Invalid credentials." });
        }


       // Logout is managed together in AccountController logout function.
    }
}
