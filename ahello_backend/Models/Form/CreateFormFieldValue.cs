namespace ahello_backend.Models.Form
{
    public class CreateFormFieldValue
    {
        public int FormId { get; set; }

        public string FieldCode { get; set; }

        public int FormFieldId { get; set; }

        public string FieldValue { get; set; }

        public string CreatedBy { get; set; }
    }
}
