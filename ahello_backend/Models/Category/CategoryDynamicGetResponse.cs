namespace ahello_backend.Models.Category
{
    public class CategoryDynamicGetResponse
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<CategoryDynamicFieldResponse> Fields { get; set; }
    }

    public class CategoryDynamicFieldResponse
    {
        public int CategoryFieldId { get; set; }

        public string FieldName { get; set; }

        public string FieldCode { get; set; }

        public string FieldValue { get; set; }
    }
}
