
namespace AdmissionPortalCreator.ViewModels
{
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; }
        public string TenantName { get; set; }
        public List<StudentDashboardFormViewModel> ActiveForms { get; set; }
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
