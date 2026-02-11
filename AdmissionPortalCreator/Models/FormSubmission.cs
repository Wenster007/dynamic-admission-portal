using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdmissionPortalCreator.Models
{
    public class FormSubmission
    {
        [Key]
        public int SubmissionId { get; set; }

        [Required]
        public int FormId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime SubmittedAt { get; set; }

        // Navigation properties
        [ForeignKey("FormId")]
        public virtual Form Form { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<FormAnswer> FormAnswers { get; set; }
    }
}