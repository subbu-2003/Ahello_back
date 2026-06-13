namespace ahello_backend.Models.Servicecategory
{
    public class ServiceCategoryDynamicPost
    {
        public int ServiceCategoryId { get; set; }
        public bool IsActive { get; set; } = true;
        public string CreatedBy { get; set; }

        public List<ServiceCategoryDynamicFieldPost>? Fields { get; set; }
    }

    public class ServiceCategoryDynamicFieldPost
    {
        public int ServiceCategoryFieldId { get; set; }

        public string FieldValue { get; set; }

        public List<ServiceCategoryDropdownOptionPost>? DropDownOptions { get; set; }
    }
    public class ServiceCategoryDropdownOptionPost
    {
        public string OptionValue { get; set; }

        public string OptionLabel { get; set; }

        public bool IsActive { get; set; }
    }
}
