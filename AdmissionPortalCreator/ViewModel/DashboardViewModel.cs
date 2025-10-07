using AdmissionPortalCreator.Models;

namespace AdmissionPortalCreator.ViewModel
{
    public class DashboardViewModel
    {
        public Tenant Tenant { get; set; }
        public List<UserViewModel> Users { get; set; }
    }
}
