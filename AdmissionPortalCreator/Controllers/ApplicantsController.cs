using AdmissionPortalCreator.Data;
using AdmissionPortalCreator.Models;
using AdmissionPortalCreator.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdmissionPortalCreator.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ApplicantsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicantsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // List all applicants for a specific form
        public async Task<IActionResult> Index(int formId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var form = await _context.Forms
                .Include(f => f.Tenant)
                .FirstOrDefaultAsync(f => f.FormId == formId && f.TenantId == user.TenantId);

            if (form == null)
                return NotFound();

            var submissions = await _context.FormSubmissions
                .Include(s => s.User)
                .Where(s => s.FormId == formId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            ViewBag.Form = form;
            return View(submissions);
        }

        // View detailed submission
        public async Task<IActionResult> ViewSubmission(int submissionId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var submission = await _context.FormSubmissions
                .Include(s => s.Form)
                    .ThenInclude(f => f.FormSections)
                        .ThenInclude(sec => sec.FormFields)
                .Include(s => s.FormAnswers)
                    .ThenInclude(a => a.FormField)
                        .ThenInclude(f => f.FieldOptions)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
                return NotFound();

            // Verify the form belongs to the current user's tenant
            if (submission.Form.TenantId != user.TenantId)
                return Forbid();

            return View(submission);
        }

        // Export submissions as CSV
        public async Task<IActionResult> ExportCSV(int formId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var form = await _context.Forms
                .Include(f => f.FormSections)
                    .ThenInclude(s => s.FormFields)
                .FirstOrDefaultAsync(f => f.FormId == formId && f.TenantId == user.TenantId);

            if (form == null)
                return NotFound();

            var submissions = await _context.FormSubmissions
                .Include(s => s.User)
                .Include(s => s.FormAnswers)
                    .ThenInclude(a => a.FormField)
                .Where(s => s.FormId == formId)
                .OrderBy(s => s.SubmittedAt)
                .ToListAsync();

            // Build CSV
            var csv = new System.Text.StringBuilder();

            // Header row
            var headers = new List<string> { "Submission ID", "Applicant Name", "Email", "Submitted On" };
            var allFields = form.FormSections
                .SelectMany(s => s.FormFields)
                .OrderBy(f => f.OrderIndex)
                .ToList();

            headers.AddRange(allFields.Select(f => f.Label));
            csv.AppendLine(string.Join(",", headers.Select(h => $"\"{h}\"")));

            // Data rows
            foreach (var submission in submissions)
            {
                var row = new List<string>
                {
                    submission.SubmissionId.ToString(),
                    $"\"{submission.User.FullName ?? submission.User.UserName}\"",
                    $"\"{submission.User.Email}\"",
                    $"\"{submission.SubmittedAt:yyyy-MM-dd HH:mm}\""
                };

                foreach (var field in allFields)
                {
                    var answer = submission.FormAnswers.FirstOrDefault(a => a.FieldId == field.FieldId);
                    var value = answer?.AnswerValue ?? "";
                    row.Add($"\"{value.Replace("\"", "\"\"")}\"");
                }

                csv.AppendLine(string.Join(",", row));
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            var fileName = $"{form.Name}_Applications_{DateTime.Now:yyyyMMdd}.csv";

            return File(bytes, "text/csv", fileName);
        }
    }
}