namespace ahello_backend.Models.Servicecategory
{
    public class ServiceCategoryDynamicPut
    {
        public string ServiceCategoryName { get; set; }
        public bool IsActive { get; set; }
        public string ModifiedBy { get; set; }

        public List<ServiceCategoryDynamicFieldPut>? Fields { get; set; }
    }

    public class ServiceCategoryDynamicFieldPut
    {
        public int ServiceCategoryFieldId { get; set; }

        public string FieldValue { get; set; }

        public List<ServiceCategoryDropdownOptionPut>? DropDownOptions{ get; set; }
    }
    public class ServiceCategoryDropdownOptionPut
    {
        public string OptionValue { get; set; }

        public string OptionLabel { get; set; }

        public bool IsActive { get; set; }
    }
}
