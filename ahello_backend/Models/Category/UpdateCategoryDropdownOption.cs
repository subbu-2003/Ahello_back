namespace ahello_backend.Models.Category
{
    public class UpdateCategoryDropdownOption
    {
        public int CategoryDropDownId { get; set; }

        public string OptionValue { get; set; }

        public string OptionLabel { get; set; }

        public bool IsActive { get; set; }

        public string ModifiedBy { get; set; }
    }
}
