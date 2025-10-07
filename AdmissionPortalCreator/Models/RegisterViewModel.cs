using System.ComponentModel.DataAnnotations;

namespace AdmissionPortalCreator.Models
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Institute Name")]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        public int? TenantId { get; set; } 
    }
}
