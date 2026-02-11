using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdmissionPortalCreator.Models
{
    public class FormAnswer
    {
        [Key]
        public int AnswerId { get; set; }

        [Required]
        public int SubmissionId { get; set; }

        [Required]
        public int FieldId { get; set; }

        public string? AnswerValue { get; set; }

        [StringLength(500)]
        public string? FilePath { get; set; }

        // Navigation properties
        [ForeignKey("SubmissionId")]
        public virtual FormSubmission FormSubmission { get; set; }

        [ForeignKey("FieldId")]
        public virtual FormField FormField { get; set; }
    }
}