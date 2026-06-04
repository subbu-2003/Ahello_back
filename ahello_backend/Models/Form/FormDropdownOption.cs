namespace ahello_backend.Models.Form
{
    public class FormDropdownOption
    {
        public int FormDropDownId { get; set; }

        public int FormFieldId { get; set; }

        public int FormId { get; set; }

        public string OptionValue { get; set; }

        public string OptionLabel { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string ModifiedBy { get; set; }
    }

    public class DropdownOptionItem
    {
        public string OptionValue { get; set; }

        public string OptionLabel { get; set; }

        public bool IsActive { get; set; }
    }

    public class CreateFormDropdownOption
    {
        public int FormFieldId { get; set; }

        public int FormId { get; set; }

        public string CreatedBy { get; set; }

        public List<DropdownOptionItem> Options { get; set; }
            = new List<DropdownOptionItem>();
    }

    public class UpdateFormDropdownOption
    {
        public int FormDropDownId { get; set; }

        public string OptionValue { get; set; }

        public string OptionLabel { get; set; }

        public bool IsActive { get; set; }

        public string ModifiedBy { get; set; }
    }

    public class GetFormDropdownOption
    {
        public int FormDropDownId { get; set; }

        public int FormFieldId { get; set; }

        public int FormId { get; set; }

        public string OptionValue { get; set; }

        public string OptionLabel { get; set; }

        public bool IsActive { get; set; }
    }
}
