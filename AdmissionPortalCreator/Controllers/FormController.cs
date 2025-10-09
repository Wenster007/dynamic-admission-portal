using AdmissionPortalCreator.Data;
using AdmissionPortalCreator.Models;
using AdmissionPortalCreator.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AdmissionPortalCreator.Controllers
{
    public class FormController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FormController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> CreateEdit(int? id)
        {
            // Get tenant ID from logged-in user
            var tenantId = await GetCurrentTenantIdAsync();

            if (tenantId == 0)
            {
                TempData["Error"] = "You are not associated with any tenant. Please contact administrator.";
                return RedirectToAction("Index", "Home");
            }

            if (id.HasValue)
            {
                // Edit mode - load existing form
                var form = await _context.Forms
                    .Include(f => f.FormSections)
                        .ThenInclude(s => s.FormFields)
                            .ThenInclude(f => f.FieldOptions)
                    .FirstOrDefaultAsync(f => f.FormId == id.Value && f.TenantId == tenantId);

                if (form == null)
                    return NotFound();

                var viewModel = MapToViewModel(form);
                return View(viewModel);
            }

            // Create mode - return empty form
            return View(new FormCreationViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEdit(FormCreationViewModel model, string sectionsJson)
        {
            if (string.IsNullOrEmpty(sectionsJson))
            {
                ModelState.AddModelError("", "Please add at least one section with fields.");
                return View(model);
            }

            try
            {
                // Get TenantId from logged-in user
                var tenantId = await GetCurrentTenantIdAsync();

                if (tenantId == 0)
                {
                    ModelState.AddModelError("", "You are not associated with any tenant. Please contact administrator.");
                    return View(model);
                }

                // Deserialize sections from JSON
                var sections = JsonSerializer.Deserialize<List<FormSectionViewModel>>(sectionsJson);
                model.Sections = sections;

                if (!ModelState.IsValid)
                    return View(model);

                if (model.FormId == 0)
                {
                    // Create new form
                    await CreateNewForm(model, tenantId);
                }
                else
                {
                    // Update existing form
                    await UpdateExistingForm(model, tenantId);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Form saved successfully!";
                return RedirectToAction("Index", "Home");
            }
            catch (DbUpdateException ex)
            {
                // Log the error
                var innerException = ex.InnerException?.Message ?? ex.Message;
                ModelState.AddModelError("", $"Database error: {innerException}");

                // Deserialize sections to restore form state
                if (!string.IsNullOrEmpty(sectionsJson))
                {
                    model.Sections = JsonSerializer.Deserialize<List<FormSectionViewModel>>(sectionsJson);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error saving form: " + ex.Message);

                // Deserialize sections to restore form state
                if (!string.IsNullOrEmpty(sectionsJson))
                {
                    model.Sections = JsonSerializer.Deserialize<List<FormSectionViewModel>>(sectionsJson);
                }

                return View(model);
            }
        }

        private async Task CreateNewForm(FormCreationViewModel model, int tenantId)
        {
            // Generate a random short code (e.g., "A7GZ3P")
            string randomCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

            // Create application website URL
            string baseUrl = $"{Request.Scheme}://{Request.Host}";
            string applicationUrl = $"{baseUrl}/apply/{tenantId}/{randomCode}";

            var form = new Form
            {
                TenantId = tenantId,
                Name = model.Name,
                Description = model.Description,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Status = model.Status,
                ApplicationWebsite = applicationUrl,
                CreatedAt = DateTime.Now,
                FormSections = new List<FormSection>()
            };

            foreach (var sectionVM in model.Sections)
            {
                var section = new FormSection
                {
                    Title = sectionVM.Title,
                    Description = sectionVM.Description ?? "",
                    OrderIndex = sectionVM.OrderIndex,
                    FormFields = new List<FormField>()
                };

                foreach (var fieldVM in sectionVM.Fields)
                {
                    var field = new FormField
                    {
                        Label = fieldVM.Label,
                        FieldType = fieldVM.FieldType,
                        IsRequired = fieldVM.IsRequired,
                        OrderIndex = fieldVM.OrderIndex,
                        FieldOptions = new List<FieldOption>()
                    };

                    if (fieldVM.Options != null && fieldVM.Options.Any())
                    {
                        foreach (var option in fieldVM.Options)
                        {
                            field.FieldOptions.Add(new FieldOption
                            {
                                OptionValue = option
                            });
                        }
                    }

                    section.FormFields.Add(field);
                }

                form.FormSections.Add(section);
            }

            _context.Forms.Add(form);
        }

        private async Task UpdateExistingForm(FormCreationViewModel model, int tenantId)
        {
            var form = await _context.Forms
                .Include(f => f.FormSections)
                    .ThenInclude(s => s.FormFields)
                        .ThenInclude(f => f.FieldOptions)
                .FirstOrDefaultAsync(f => f.FormId == model.FormId && f.TenantId == tenantId);

            if (form == null)
                throw new Exception("Form not found or access denied");

            form.Name = model.Name;
            form.Description = model.Description;
            form.StartDate = model.StartDate;
            form.EndDate = model.EndDate;
            form.Status = model.Status;
            form.ApplicationWebsite = model.ApplicationWebsite;


            var existingSectionIds = model.Sections
                .Where(s => s.SectionId > 0)
                .Select(s => s.SectionId)
                .ToList();

            var sectionsToRemove = form.FormSections
                .Where(s => !existingSectionIds.Contains(s.SectionId))
                .ToList();

            foreach (var section in sectionsToRemove)
            {
                _context.FormSections.Remove(section);
            }

            foreach (var sectionVM in model.Sections)
            {
                if (sectionVM.SectionId > 0)
                {
                    var existingSection = form.FormSections
                        .FirstOrDefault(s => s.SectionId == sectionVM.SectionId);

                    if (existingSection != null)
                    {
                        existingSection.Title = sectionVM.Title;
                        existingSection.Description = sectionVM.Description ?? "";
                        existingSection.OrderIndex = sectionVM.OrderIndex;
                        UpdateSectionFields(existingSection, sectionVM.Fields);
                    }
                }
                else
                {
                    var newSection = new FormSection
                    {
                        FormId = form.FormId,
                        Title = sectionVM.Title,
                        Description = sectionVM.Description ?? "",
                        OrderIndex = sectionVM.OrderIndex,
                        FormFields = new List<FormField>()
                    };

                    foreach (var fieldVM in sectionVM.Fields)
                    {
                        var field = new FormField
                        {
                            Label = fieldVM.Label,
                            FieldType = fieldVM.FieldType,
                            IsRequired = fieldVM.IsRequired,
                            OrderIndex = fieldVM.OrderIndex,
                            FieldOptions = new List<FieldOption>()
                        };

                        if (fieldVM.Options != null && fieldVM.Options.Any())
                        {
                            foreach (var option in fieldVM.Options)
                            {
                                field.FieldOptions.Add(new FieldOption
                                {
                                    OptionValue = option
                                });
                            }
                        }

                        newSection.FormFields.Add(field);
                    }

                    form.FormSections.Add(newSection);
                }
            }
        }

        private void UpdateSectionFields(FormSection section, List<FormFieldViewModel> fieldVMs)
        {
            var existingFieldIds = fieldVMs
                .Where(f => f.FieldId > 0)
                .Select(f => f.FieldId)
                .ToList();

            var fieldsToRemove = section.FormFields
                .Where(f => !existingFieldIds.Contains(f.FieldId))
                .ToList();

            foreach (var field in fieldsToRemove)
            {
                _context.FormFields.Remove(field);
            }

            foreach (var fieldVM in fieldVMs)
            {
                if (fieldVM.FieldId > 0)
                {
                    var existingField = section.FormFields
                        .FirstOrDefault(f => f.FieldId == fieldVM.FieldId);

                    if (existingField != null)
                    {
                        existingField.Label = fieldVM.Label;
                        existingField.FieldType = fieldVM.FieldType;
                        existingField.IsRequired = fieldVM.IsRequired;
                        existingField.OrderIndex = fieldVM.OrderIndex;
                        UpdateFieldOptions(existingField, fieldVM.Options);
                    }
                }
                else
                {
                    var newField = new FormField
                    {
                        SectionId = section.SectionId,
                        Label = fieldVM.Label,
                        FieldType = fieldVM.FieldType,
                        IsRequired = fieldVM.IsRequired,
                        OrderIndex = fieldVM.OrderIndex,
                        FieldOptions = new List<FieldOption>()
                    };

                    if (fieldVM.Options != null && fieldVM.Options.Any())
                    {
                        foreach (var option in fieldVM.Options)
                        {
                            newField.FieldOptions.Add(new FieldOption
                            {
                                OptionValue = option
                            });
                        }
                    }

                    section.FormFields.Add(newField);
                }
            }
        }

        private void UpdateFieldOptions(FormField field, List<string> options)
        {
            _context.FieldOptions.RemoveRange(field.FieldOptions);
            field.FieldOptions.Clear();

            if (options != null && options.Any())
            {
                foreach (var option in options)
                {
                    field.FieldOptions.Add(new FieldOption
                    {
                        FieldId = field.FieldId,
                        OptionValue = option
                    });
                }
            }
        }

        private FormCreationViewModel MapToViewModel(Form form)
        {
            return new FormCreationViewModel
            {
                FormId = form.FormId,
                Name = form.Name,
                Description = form.Description,
                StartDate = form.StartDate,
                EndDate = form.EndDate,
                Status = form.Status,
                ApplicationWebsite = form.ApplicationWebsite,
                Sections = form.FormSections.OrderBy(s => s.OrderIndex).Select(s => new FormSectionViewModel
                {
                    SectionId = s.SectionId,
                    Title = s.Title,
                    Description = s.Description,
                    OrderIndex = s.OrderIndex,
                    Fields = s.FormFields.OrderBy(f => f.OrderIndex).Select(f => new FormFieldViewModel
                    {
                        FieldId = f.FieldId,
                        Label = f.Label,
                        FieldType = f.FieldType,
                        IsRequired = f.IsRequired,
                        OrderIndex = f.OrderIndex,
                        Options = f.FieldOptions.Select(o => o.OptionValue).ToList()
                    }).ToList()
                }).ToList()
            };
        }

        // THE CORRECT WAY: Get TenantId from logged-in user
        private async Task<int> GetCurrentTenantIdAsync()
        {
            if (!User.Identity.IsAuthenticated)
                return 0;

            // Get the current logged-in user
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return 0;

            // Return the TenantId from the user's record
            return user.TenantId ?? 0;
        }

        public async Task<IActionResult> Index()
        {
            var tenantId = await GetCurrentTenantIdAsync();

            if (tenantId == 0)
            {
                TempData["Error"] = "You are not associated with any tenant.";
                return View(new List<Form>());
            }

            var forms = await _context.Forms
                .Include(f => f.Tenant)
                .Where(f => f.TenantId == tenantId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(forms);
        }
    }
}