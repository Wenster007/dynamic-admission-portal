namespace AdmissionPortalCreator.Models
{
    public class Form
    {
        public int FormId { get; set; }
        public int TenantId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ApplicationWebsite { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Tenant Tenant { get; set; }
        public virtual ICollection<FormSection> FormSections { get; set; }
    }
}
