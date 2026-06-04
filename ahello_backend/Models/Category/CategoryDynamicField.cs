namespace ahello_backend.Models.Category
{
    public class CategoryDynamicField
    {
        public int CategoryFieldId { get; set; }

        public string FieldName { get; set; }

        public string FieldCode { get; set; }

        public string Placeholder { get; set; }

        public int DataTypeId { get; set; }

        public bool IsRequired { get; set; }

        public List<CategoryDropdownOption>? Options { get; set; }
    }
}
