namespace AdmissionPortalCreator.Models
{
    public class FormSubmission
    {
        public int SubmissionId { get; set; }
        public int FormId { get; set; }
        public string UserId { get; set; } // Changed to string to match Identity
        public DateTime SubmittedAt { get; set; }

        // Navigation properties
        public virtual Form Form { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<FormAnswer> FormAnswers { get; set; }
    }
}
