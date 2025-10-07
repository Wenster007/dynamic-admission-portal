namespace AdmissionPortalCreator.Models
{
    public class FieldOption
    {
        public int OptionId { get; set; }
        public int FieldId { get; set; }
        public string OptionValue { get; set; }

        public virtual FormField FormField { get; set; }
    }
}
