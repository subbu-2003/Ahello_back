namespace ahello_backend.Models.Category
{
    public class CreateCategoryDropdownOption
    {
        public int CategoryFieldId { get; set; }

        public int CategoryId { get; set; }

        public string CreatedBy { get; set; }

        public List<CategoryDropdownOptionItem> Options { get; set; }
            = new List<CategoryDropdownOptionItem>();
    }

    public class CategoryDropdownOptionItem
    {
        public string OptionValue { get; set; }

        public string OptionLabel { get; set; }

        public bool IsActive { get; set; }
    }
}
