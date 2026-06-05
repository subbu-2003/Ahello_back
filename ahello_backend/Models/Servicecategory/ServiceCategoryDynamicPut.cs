namespace ahello_backend.Models.Servicecategory
{
    public class ServiceCategoryDynamicPut
    {
        public string ModifiedBy { get; set; }

        public List<ServiceCategoryDynamicFieldPut>? Fields { get; set; }
    }

    public class ServiceCategoryDynamicFieldPut
    {
        public int ServiceCategoryFieldId { get; set; }

        public string FieldValue { get; set; }
    }
}
