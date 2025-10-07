namespace AdmissionPortalCreator.Models
{
    public class FormField
    {
        public int FieldId { get; set; }
        public int SectionId { get; set; }
        public string Label { get; set; }
        public string FieldType { get; set; }
        public bool IsRequired { get; set; }
        public int OrderIndex { get; set; }

        public virtual FormSection FormSection { get; set; }
        public virtual ICollection<FieldOption> FieldOptions { get; set; }
    }
}
