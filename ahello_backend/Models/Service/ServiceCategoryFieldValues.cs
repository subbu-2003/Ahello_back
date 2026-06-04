namespace ahello_backend.Models.Service
{
    public class ServiceCategoryFieldValues
    {
        public int ServiceCategoryFieldValueId { get; set; }

        public int ServiceCategoryId { get; set; }

        public string? FieldCode { get; set; }

        public int ServiceCategoryFieldId { get; set; }

        public string? FieldValue { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }
    }
}
