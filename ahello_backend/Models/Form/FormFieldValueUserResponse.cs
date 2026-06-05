namespace ahello_backend.Models.Form
{
    public class FormFieldValueUserResponse
    {
        public int FormId { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public int CreatedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public int? ModifiedBy { get; set; }

        public List<FormFieldDetailResponse> Fields { get; set; }
            = new();
    }

    public class FormFieldDetailResponse
    {
        public int FormFieldId { get; set; }

        public int FormId { get; set; }

        public string FieldName { get; set; }

        public string FieldCode { get; set; }

        public string Placeholder { get; set; }

        public string Description { get; set; }

        public bool IsRequired { get; set; }

        public bool IsActive { get; set; }

        public int DataTypeId { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public List<FormFieldValue> FieldValues { get; set; }
            = new();
    }
}
