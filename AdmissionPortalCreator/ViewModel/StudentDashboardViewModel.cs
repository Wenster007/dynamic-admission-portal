
using AdmissionPortalCreator.Models;

namespace AdmissionPortalCreator.ViewModels
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; }
        public string TenantName { get; set; }
        public List<Form> ActiveForms { get; set; } = new List<Form>();
        public List<int> SubmittedFormIds { get; set; } = new List<int>();
        public List<FormSubmission> Submissions { get; set; } = new List<FormSubmission>();
    }

    public class StudentDashboardFormViewModel
    {
        public int FormId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public string TenantName { get; set; }
    }
}
