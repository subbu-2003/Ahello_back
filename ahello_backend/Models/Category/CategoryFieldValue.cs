namespace ahello_backend.Models.Category
{
    public class CategoryFieldValue
    {
        public int CategoryFieldValueId { get; set; }

        public int CategoryId { get; set; }

        public string FieldCode { get; set; }

        public int CategoryFieldId { get; set; }

        public string FieldValue { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime ModifiedAt { get; set; }
    }
}
