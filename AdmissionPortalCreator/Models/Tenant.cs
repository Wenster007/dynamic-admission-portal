using System.ComponentModel.DataAnnotations;

namespace AdmissionPortalCreator.Models
{
    public class Tenant
    {
        [Key]
        public int TenantId { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Address { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Phone { get; set; }

        // e.g. "uni123.mysite.com" or "/uni123"
        public string? SubdomainOrUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
