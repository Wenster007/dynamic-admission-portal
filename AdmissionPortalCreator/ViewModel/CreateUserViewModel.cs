using System.ComponentModel.DataAnnotations;

namespace AdmissionPortalCreator.ViewModel
{
    public class CreateUserViewModel
    {
        public string? Id { get; set; }

        public string Email { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string SelectedRole { get; set; }
        public List<string> AvailableRoles { get; set; } = new();

        public string Mode { get; set; } = "Create"; // default
    }

}
