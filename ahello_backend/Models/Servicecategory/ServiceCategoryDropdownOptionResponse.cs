namespace ahello_backend.Models.Servicecategory
{
    public class ServiceCategoryDropdownOptionResponse
    {
        public int ServiceCategoryDropDownId { get; set; }

        public int ServiceCategoryFieldId { get; set; }

        public int ServiceCategoryId { get; set; }

        public string OptionValue { get; set; }

        public string OptionLabel { get; set; }

        public bool IsActive { get; set; }
    }
}