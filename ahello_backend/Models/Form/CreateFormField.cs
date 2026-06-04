namespace ahello_backend.Models.Form
{
    public class CreateFormField
    {
        public int FormId { get; set; }

        public string FieldName { get; set; }

        public string FieldCode { get; set; }

        public string Placeholder { get; set; }

        public string Description { get; set; }

        public bool IsRequired { get; set; }

        public bool IsActive { get; set; }

        public int DataTypeId { get; set; }

        public string CreatedBy { get; set; }
    }
}
