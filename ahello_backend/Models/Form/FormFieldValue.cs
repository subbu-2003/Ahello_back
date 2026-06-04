namespace ahello_backend.Models.Form
{
    public class FormFieldValue
    {
        public int FormFieldValueId { get; set; }

        public int FormId { get; set; }

        public string FieldCode { get; set; }

        public int FormFieldId { get; set; }

        public string FieldValue { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }
    }
}
