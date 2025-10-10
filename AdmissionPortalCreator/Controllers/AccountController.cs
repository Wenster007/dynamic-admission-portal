using AdmissionPortalCreator.Data;
using AdmissionPortalCreator.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace AdmissionPortalCreator.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;


        public AccountController(SignInManager<ApplicationUser> signInManager,
                                 UserManager<ApplicationUser> userManager, ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }

        // 🔹 GET: Login Page
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                // already logged in → send them to Home page
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel()); // sends empty model to view
        }

        // 🔹 POST: Login Form Submission
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home"); // redirect after login
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View(model);
        }

         //🔹 GET: Register Page
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // 🔹 POST: Register Form Submission
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1. Create Tenant
            var tenant = new Tenant
            {
                Name = model.Name,
                Email = model.Email,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            // 2. Create User linked to Tenant
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                TenantId = tenant.TenantId,
                FullName = "Admin", // first user will be admin
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // 3. Ensure global roles exist
                if (!await _roleManager.RoleExistsAsync("Admin"))
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));

                if (!await _roleManager.RoleExistsAsync("Manager"))
                    await _roleManager.CreateAsync(new IdentityRole("Manager"));

                if (!await _roleManager.RoleExistsAsync("Student"))
                    await _roleManager.CreateAsync(new IdentityRole("Student"));

                // 4. Assign Admin role to first user of tenant
                await _userManager.AddToRoleAsync(user, "Admin");

                // 5. Auto-login
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            // Show errors if creation failed
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        // 🔹 Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}
