using AdmissionPortalCreator.Data;
using AdmissionPortalCreator.Models;
using AdmissionPortalCreator.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdmissionPortalCreator.Controllers
{
    [Authorize(Roles = "Student,Admin,Manager")]
    public class StudentDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // -----------------------------------------
        // GET: /Student/Dashboard
        // -----------------------------------------
        public async Task<IActionResult> Dashboard()
        {
            // Get current logged-in student
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "StudentAccount");

            // Get tenantId of student
            var tenantId = user.TenantId;

            // Get active forms for this tenant
            var currentDate = DateTime.UtcNow;

            var activeForms = await _context.Forms
                .Where(f => f.TenantId == tenantId &&
                            f.StartDate <= currentDate &&
                            f.EndDate >= currentDate)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new StudentDashboardFormViewModel
                {
                    FormId = f.FormId,
                    Name = f.Name,
                    Description = f.Description,
                    StartDate = f.StartDate,
                    EndDate = f.EndDate,
                    TenantName = f.Tenant.Name,
                    Status = f.Status
                })
                .ToListAsync();

            var model = new StudentDashboardViewModel
            {
                StudentName = user.FullName,
                TenantName = await _context.Tenants
                    .Where(t => t.TenantId == tenantId)
                    .Select(t => t.Name)
                    .FirstOrDefaultAsync(),
                ActiveForms = activeForms
            };

            return View(model);
        }

        // -----------------------------------------
        // GET: /Student/ApplyForm/{formId}
        // -----------------------------------------
        [HttpGet]
        public async Task<IActionResult> ApplyForm(int formId)
        {
            var form = await _context.Forms
             .Include(f => f.FormSections)
                 .ThenInclude(s => s.FormFields)
             .FirstOrDefaultAsync(f => f.FormId == formId);


            if (form == null)
                return NotFound("Form not found.");

            // You can later show the form fields dynamically
            return View(form);
        }
    }
}
