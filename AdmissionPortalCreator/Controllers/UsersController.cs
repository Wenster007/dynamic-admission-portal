using AdmissionPortalCreator.Models;
using AdmissionPortalCreator.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AdmissionPortalCreator.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ---------------- CREATE ----------------
        [HttpGet]
        public IActionResult Create()
        {
            var roles = _roleManager.Roles
                .Where(r => r.Name != "Student" && r.Name != "Admin")
                .Select(r => r.Name)
                .ToList();

            var model = new CreateUserViewModel
            {
                AvailableRoles = roles,
                Mode = "Create"
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (model.Mode == "Create" && ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) return Unauthorized();

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    TenantId = currentUser.TenantId
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.SelectedRole);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    if (error.Code.Contains("Password"))
                        ModelState.AddModelError(nameof(model.Password), error.Description);
                    else if (error.Code.Contains("Email") || error.Code.Contains("UserName"))
                        ModelState.AddModelError(nameof(model.Email), error.Description);
                    else
                        ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // reload roles if model invalid
            model.AvailableRoles = _roleManager.Roles
                .Where(r => r.Name != "Student" && r.Name != "Admin")
                .Select(r => r.Name)
                .ToList();

            return View(model);
        }

        // ---------------- EDIT PASSWORD ----------------
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var model = new CreateUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                SelectedRole = roles.FirstOrDefault(),
                Mode = "EditPassword"
            };

            return View("Create", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CreateUserViewModel model)
        {
            if (model.Mode == "EditPassword" && ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound();

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.Password);

                if (result.Succeeded)
                    return RedirectToAction("Index", "Home");

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(nameof(model.Password), error.Description);
                }
            }

            return View("Create", model);
        }

        // ---------------- DELETE ----------------
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            Console.WriteLine(" ljkljklkj ");
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var model = new CreateUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                SelectedRole = roles.FirstOrDefault(),
                Mode = "Delete"
            };

            return View("Create", model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(CreateUserViewModel model)
        {
            Console.WriteLine(" ljkljklkj " + model.Password);

            if (model.Mode == "Delete")
            {
                // Clear all ModelState errors for Delete mode
                ModelState.Clear();

                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound();

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                    return RedirectToAction("Index", "Home");

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we get here, something went wrong - reload the model
            if (model.Mode == "Delete")
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    model.SelectedRole = roles.FirstOrDefault();
                }
            }

            return View("Create", model);
        }
    }
}
