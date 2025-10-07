using Microsoft.AspNetCore.Http;

namespace AdmissionPortalCreator.Models
{
    public class FormSection
    {
        public int SectionId { get; set; }
        public int FormId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int OrderIndex { get; set; }

        public virtual Form Form { get; set; }
        public virtual ICollection<FormField> FormFields { get; set; }
    }
}
