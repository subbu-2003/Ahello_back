namespace ahello_backend.Models.Service
{
    public class ServiceCategoryFields
    {
        public int ServiceCategoryFieldId { get; set; }

        //public int ServiceCategoryId { get; set; }

        public string FieldName { get; set; } = string.Empty;

        public string FieldCode { get; set; } = string.Empty;

        public string? Placeholder { get; set; }

        public bool IsRequired { get; set; }

        public bool IsActive { get; set; }

        public int DataTypeId { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }
    }
}
