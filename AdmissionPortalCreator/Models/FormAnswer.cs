namespace AdmissionPortalCreator.Models
{
    public class FormAnswer
    {
        public int AnswerId { get; set; }
        public int SubmissionId { get; set; }
        public int FieldId { get; set; }
        public string AnswerValue { get; set; }
        public string FilePath { get; set; }

        public virtual FormSubmission FormSubmission { get; set; }
        public virtual FormField FormField { get; set; }
    }
}
