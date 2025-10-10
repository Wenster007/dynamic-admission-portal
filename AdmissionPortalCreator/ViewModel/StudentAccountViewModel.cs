namespace AdmissionPortalCreator.ViewModels
{
    public class StudentAccountViewModel
    {
        public int TenantId { get; set; }
        public string FormCode { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;

        // Common
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Register-only
        public string FullName { get; set; } = string.Empty;

        // UI control
        public bool IsLoginMode { get; set; } = true;
    }
}
