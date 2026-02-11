using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdmissionPortalCreator.Models
{
    public class Form
    {
        [Key]
        public int FormId { get; set; }

        [Required]
        public int TenantId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [StringLength(500)]
        public string? ApplicationWebsite { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } // "Draft", "Active", "Paused"

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [ForeignKey("TenantId")]
        public virtual Tenant Tenant { get; set; }

        public virtual ICollection<FormSection> FormSections { get; set; }
        public virtual ICollection<FormSubmission> FormSubmissions { get; set; }
    }
}