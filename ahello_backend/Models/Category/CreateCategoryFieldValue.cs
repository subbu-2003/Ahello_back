namespace ahello_backend.Models.Category
{
    public class CreateCategoryFieldValue
    {
        public int CategoryId { get; set; }

        public string FieldCode { get; set; }

        public int CategoryFieldId { get; set; }

        public string FieldValue { get; set; }

        public string CreatedBy { get; set; }
    }

}
