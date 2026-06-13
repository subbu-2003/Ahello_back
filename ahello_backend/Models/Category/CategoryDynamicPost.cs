namespace ahello_backend.Models.Category
{
    public class CategoryDynamicPost
    {
        public string CategoryName { get; set; }

        public string CreatedBy { get; set; }
        public bool IsActive { get; set; } = true;
        public List<CategoryDynamicFieldPost> Fields { get; set; }
    }

    public class CategoryDynamicFieldPost
    {
        public int CategoryFieldId { get; set; }

        public string FieldValue { get; set; }
    }
}
