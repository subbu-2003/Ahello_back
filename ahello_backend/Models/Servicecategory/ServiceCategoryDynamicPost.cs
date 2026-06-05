namespace ahello_backend.Models.Servicecategory
{
    public class ServiceCategoryDynamicPost
    {
        public int ServiceCategoryId { get; set; }

        public string CreatedBy { get; set; }

        public List<ServiceCategoryDynamicFieldPost>? Fields { get; set; }
    }

    public class ServiceCategoryDynamicFieldPost
    {
        public int ServiceCategoryFieldId { get; set; }

        public string FieldValue { get; set; }
    }
}
