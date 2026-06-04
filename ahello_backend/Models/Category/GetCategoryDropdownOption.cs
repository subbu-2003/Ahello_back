namespace ahello_backend.Models.Category
{
    public class GetCategoryDropdownOption
    {
        public int CategoryDropDownId { get; set; }

        public int CategoryFieldId { get; set; }

        public int CategoryId { get; set; }

        public string OptionValue { get; set; }

        public string OptionLabel { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string? ModifiedBy { get; set; }
    }
}
