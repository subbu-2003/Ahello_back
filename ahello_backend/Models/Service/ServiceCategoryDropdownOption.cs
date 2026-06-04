namespace ahello_backend.Models.Service
{
    public class ServiceCategoryDropdownOption
    {
        public int ServiceCategoryDropDownId { get; set; }

        public int ServiceCategoryFieldId { get; set; }

        public int ServiceCategoryId { get; set; }

        public string? OptionValue { get; set; }

        public string? OptionLabel { get; set; }

        public bool IsActive { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string? ModifiedBy { get; set; }
    }
}
