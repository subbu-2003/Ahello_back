namespace ahello_backend.Models.Servicecategory
{
    public class ServiceCategoryDynamicGetResponse
    {
        public int ServiceCategoryId { get; set; }
        public bool IsActive { get; set; }
        public string ServiceCategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ServiceCategoryDynamicFieldResponse>? Fields { get; set; }
        public List<ServiceCategoryDropdownOptionResponse>? DropdownOptions { get; set; }
    }

    public class ServiceCategoryDynamicFieldResponse
    {
        public int ServiceCategoryFieldId { get; set; }

        public string FieldName { get; set; }

        public string FieldCode { get; set; }

        public string Placeholder { get; set; }

        public bool IsRequired { get; set; }

        public int DataTypeId { get; set; }

        public string FieldValue { get; set; }
    }
}
