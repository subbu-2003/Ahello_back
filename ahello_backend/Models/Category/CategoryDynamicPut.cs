namespace ahello_backend.Models.Category
{
    public class CategoryDynamicPut
    {
        public string CategoryName { get; set; }

        public string ModifiedBy { get; set; }

        public List<CategoryDynamicFieldPut> Fields { get; set; }
    }

    public class CategoryDynamicFieldPut
    {
        public int CategoryFieldId { get; set; }

        public string FieldValue { get; set; }
    }
}
