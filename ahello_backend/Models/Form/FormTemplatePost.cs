namespace ahello_backend.Models.Form
{
    public class FormTemplatePost
    {
        public int UserId { get; set; }
        public int CreatedBy { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<FormTemplateFieldPost> Fields { get; set; }
    }

    public class FormTemplateFieldPost
    {
        public string FieldName { get; set; }

        public string Placeholder { get; set; }

        public string Description { get; set; }

        public bool IsRequired { get; set; }

        public int DataTypeId { get; set; }

        public List<FormTemplateDropdownPost> DropdownOptions { get; set; }
    }

    public class FormTemplateDropdownPost
    {
        public string OptionLabel { get; set; }
    }
}
