namespace ahello_backend.Models.Category
{
    public class CategoryBasicField
    {
        public string FieldCode { get; set; }

        public string Label { get; set; }

        public string DataType { get; set; }

        public bool IsRequired { get; set; }

        public List<CategoryDropdownOption>? Options { get; set; }
    }
}
