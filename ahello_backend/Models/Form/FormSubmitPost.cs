namespace ahello_backend.Models.Form
{
    public class FormSubmitPost
    {
        public int FormId { get; set; }

        public List<FormFieldValuePost> FormFieldValues { get; set; }

        public List<FormDropdownValuePost> FormDropdownOptions { get; set; }
    }

    public class FormFieldValuePost
    {
        public int FormFieldId { get; set; }

        public string FieldValue { get; set; }
    }

    public class FormDropdownValuePost
    {
        public int FormFieldId { get; set; }

        public string OptionValue { get; set; }
    }
}

