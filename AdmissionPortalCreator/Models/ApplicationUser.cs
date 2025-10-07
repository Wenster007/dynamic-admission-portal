using Microsoft.AspNetCore.Identity;
using System;

namespace AdmissionPortalCreator.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
