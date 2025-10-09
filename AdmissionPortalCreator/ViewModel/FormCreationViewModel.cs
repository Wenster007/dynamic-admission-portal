namespace AdmissionPortalCreator.ViewModel
{
    // ViewModels/FormCreationViewModel.cs
    public class FormCreationViewModel
    {
        public int FormId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? ApplicationWebsite { get; set; }
        public string Status { get; set; }
        public List<FormSectionViewModel> Sections { get; set; } = new();
    }

    public class FormSectionViewModel
    {
        public int SectionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int OrderIndex { get; set; }
        public List<FormFieldViewModel> Fields { get; set; } = new();
    }

    public class FormFieldViewModel
    {
        public int FieldId { get; set; }
        public string Label { get; set; }
        public string FieldType { get; set; }
        public bool IsRequired { get; set; }
        public int OrderIndex { get; set; }
        public List<string> Options { get; set; } = new(); // for dropdown/radio
    }
}
