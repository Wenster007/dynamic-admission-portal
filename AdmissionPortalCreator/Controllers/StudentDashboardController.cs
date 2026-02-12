using AdmissionPortalCreator.Data;
using AdmissionPortalCreator.Models;
using AdmissionPortalCreator.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdmissionPortalCreator.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentDashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var tenantId = user.TenantId;
            var currentDate = DateTime.Now;

            // Get tenant name
            var tenantName = await _context.Tenants
                .Where(t => t.TenantId == tenantId)
                .Select(t => t.Name)
                .FirstOrDefaultAsync();

            // Get active forms for this tenant
            var activeForms = await _context.Forms
                .Where(f => f.TenantId == tenantId &&
                           f.StartDate <= currentDate &&
                           f.EndDate >= currentDate)
                .OrderBy(f => f.CreatedAt)
                .ToListAsync();

            // ✅ Get all submissions by this student
            var submissions = await _context.FormSubmissions
                .Where(s => s.UserId == user.Id)
                .ToListAsync();

            // ✅ Get list of form IDs the student has already submitted
            var submittedFormIds = submissions.Select(s => s.FormId).ToList();

            var model = new StudentDashboardViewModel
            {
                StudentName = user.FullName ?? user.UserName,
                TenantName = tenantName ?? "Unknown Institution",
                ActiveForms = activeForms,
                SubmittedFormIds = submittedFormIds,
                Submissions = submissions
            };

            return View(model);
        }
    }
}