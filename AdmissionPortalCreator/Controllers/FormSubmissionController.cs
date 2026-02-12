using AdmissionPortalCreator.Data;
using AdmissionPortalCreator.Models;
using AdmissionPortalCreator.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdmissionPortalCreator.Controllers
{
    public class FormSubmissionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FormSubmissionController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // Helper method to redirect based on user role
        private IActionResult RedirectToUserDashboard()
        {
            Console.WriteLine(User.IsInRole("Student"));
            if (User.IsInRole("Student"))
            {
                return RedirectToAction("Dashboard", "StudentDashboard");
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ApplyForm(int formId)
        {

            var currentUser = await _userManager.GetUserAsync(User);

                var form = await _context.Forms
                    .Include(f => f.FormSections)
                        .ThenInclude(s => s.FormFields)
                            .ThenInclude(fld => fld.FieldOptions)
                    .FirstOrDefaultAsync(f => f.FormId == formId);

            if (form == null)
                return NotFound();

            // Check if form is active and within date range
            if (form.Status != "Active")
            {
                TempData["Error"] = "This form is not currently accepting submissions.";
                return RedirectToUserDashboard();
            }

            if (DateTime.Now < form.StartDate || DateTime.Now > form.EndDate)
            {
                TempData["Error"] = "This form is not currently open for submissions.";
                return RedirectToUserDashboard();
            }

            // Check if user is authenticated (for students)
            if (User.Identity.IsAuthenticated)
            {
                if (currentUser != null)
                {
                    // Check if student has already submitted this form
                    var existingSubmission = await _context.FormSubmissions
                        .AnyAsync(fs => fs.FormId == formId && fs.UserId == currentUser.Id);

                    if (existingSubmission)
                    {
                        TempData["Error"] = "You have already submitted this form.";
                        return RedirectToUserDashboard();
                    }
                }
            }

            var model = new FormSubmissionViewModel
            {
                FormId = form.FormId,
                FormName = form.Name,
                FormDescription = form.Description,
                Sections = form.FormSections.OrderBy(s => s.OrderIndex)
                    .Select(s => new FormSectionViewModel
                    {
                        SectionId = s.SectionId,
                        Title = s.Title,
                        Description = s.Description,
                        Fields = s.FormFields.OrderBy(f => f.OrderIndex)
                            .Select(f => new FormFieldViewModel
                            {
                                FieldId = f.FieldId,
                                Label = f.Label,
                                FieldType = f.FieldType,
                                IsRequired = f.IsRequired,
                                Options = f.FieldOptions.Select(o => o.OptionValue).ToList()
                            }).ToList()
                    }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForm(int formId)
        {
            try
            {
                // Get the current user
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "You must be logged in to submit an application.";
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("ApplyForm", new { formId }) });
                }

                // Verify the form exists and is active
                var form = await _context.Forms
                    .Include(f => f.FormSections)
                        .ThenInclude(s => s.FormFields)
                    .FirstOrDefaultAsync(f => f.FormId == formId);

                if (form == null)
                {
                    TempData["Error"] = "Form not found.";
                    return RedirectToUserDashboard();
                }

                // Check if form is still accepting submissions
                if (form.Status != "Active")
                {
                    TempData["Error"] = "This form is not currently accepting submissions.";
                    return RedirectToUserDashboard();
                }

                if (DateTime.Now < form.StartDate || DateTime.Now > form.EndDate)
                {
                    TempData["Error"] = "This form is no longer accepting submissions.";
                    return RedirectToUserDashboard();
                }

                // Check if user has already submitted this form
                var existingSubmission = await _context.FormSubmissions
                    .AnyAsync(fs => fs.FormId == formId && fs.UserId == user.Id);

                if (existingSubmission)
                {
                    TempData["Error"] = "You have already submitted this form.";
                    return RedirectToUserDashboard();
                }

                // Create the submission record
                var submission = new FormSubmission
                {
                    FormId = formId,
                    UserId = user.Id,
                    SubmittedAt = DateTime.Now
                };

                _context.FormSubmissions.Add(submission);
                await _context.SaveChangesAsync(); // Save to get the SubmissionId

                // Process all form fields and save answers
                var allFields = form.FormSections
                    .SelectMany(s => s.FormFields)
                    .ToList();

                var hasErrors = false;
                var errorMessages = new List<string>();

                foreach (var field in allFields)
                {
                    // Handle file uploads
                    if (field.FieldType.ToLower() == "file")
                    {
                        var fileKey = $"file_{field.FieldId}";
                        var file = Request.Form.Files[fileKey];

                        if (file != null && file.Length > 0)
                        {
                            // Validate file size (5MB limit)
                            if (file.Length > 5 * 1024 * 1024)
                            {
                                errorMessages.Add($"File for '{field.Label}' exceeds 5MB limit.");
                                hasErrors = true;
                                continue;
                            }

                            // Save the file
                            var filePath = await SaveFileAsync(file, user.Id, submission.SubmissionId, field.FieldId);

                            var answer = new FormAnswer
                            {
                                SubmissionId = submission.SubmissionId,
                                FieldId = field.FieldId,
                                FilePath = filePath,
                                AnswerValue = file.FileName
                            };
                            _context.FormAnswers.Add(answer);
                        }
                        else if (field.IsRequired)
                        {
                            errorMessages.Add($"File for '{field.Label}' is required.");
                            hasErrors = true;
                        }
                    }
                    else
                    {
                        // Handle regular form fields
                        var fieldKey = $"field_{field.FieldId}";
                        var value = Request.Form[fieldKey].ToString();

                        if (field.IsRequired && string.IsNullOrWhiteSpace(value))
                        {
                            errorMessages.Add($"'{field.Label}' is required.");
                            hasErrors = true;
                            continue;
                        }

                        // Only save answer if there's a value
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            var answer = new FormAnswer
                            {
                                SubmissionId = submission.SubmissionId,
                                FieldId = field.FieldId,
                                AnswerValue = value
                            };
                            _context.FormAnswers.Add(answer);
                        }
                    }
                }

                // If there are validation errors, rollback and show errors
                if (hasErrors)
                {
                    _context.FormSubmissions.Remove(submission);
                    await _context.SaveChangesAsync();

                    TempData["Error"] = string.Join("<br/>", errorMessages);
                    return RedirectToAction("ApplyForm", new { formId });
                }

                // Save all answers
                await _context.SaveChangesAsync();

                TempData["Success"] = "Your application has been submitted successfully!";
                return RedirectToAction("SubmissionSuccess", new { submissionId = submission.SubmissionId });
            }
            catch (DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                TempData["Error"] = $"Database error: {innerMessage}";
                return RedirectToAction("ApplyForm", new { formId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("ApplyForm", new { formId });
            }
        }

        private async Task<string> SaveFileAsync(IFormFile file, string userId, int submissionId, int fieldId)
        {
            // Create directory structure: wwwroot/uploads/{userId}/{submissionId}/
            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", userId, submissionId.ToString());

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate unique filename to avoid conflicts
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"field_{fieldId}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path for database storage
            return $"/uploads/{userId}/{submissionId}/{uniqueFileName}";
        }

        [HttpGet]
        public async Task<IActionResult> SubmissionSuccess(int submissionId)
        {
            var submission = await _context.FormSubmissions
                .Include(s => s.Form)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
                return NotFound();

            // Verify the current user owns this submission
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id != submission.UserId)
                return Forbid();

            return View(submission);
        }

        [HttpGet]
        public async Task<IActionResult> MySubmissions()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var submissions = await _context.FormSubmissions
                .Include(s => s.Form)
                .Where(s => s.UserId == user.Id)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            return View(submissions);
        }

        [HttpGet]
        public async Task<IActionResult> ViewSubmission(int submissionId)
        {
            var submission = await _context.FormSubmissions
                .Include(s => s.Form)
                    .ThenInclude(f => f.FormSections)
                        .ThenInclude(sec => sec.FormFields)
                .Include(s => s.FormAnswers)
                    .ThenInclude(a => a.FormField)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null)
                return NotFound();

            // Verify the current user owns this submission OR is admin/manager
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            // Allow access if: student viewing own submission OR admin/manager viewing any submission
            var isOwnSubmission = currentUser.Id == submission.UserId;
            var isAdminOrManager = User.IsInRole("Admin") || User.IsInRole("Manager");

            if (!isOwnSubmission && !isAdminOrManager)
                return Forbid();

            return View(submission);
        }
    }
}