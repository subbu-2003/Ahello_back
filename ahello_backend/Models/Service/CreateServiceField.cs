namespace ahello_backend.Models.Service
{
    public class CreateServiceField
    {
        public int ServiceId { get; set; }

        public string FieldName { get; set; }

        public string FieldCode { get; set; }

        public string Placeholder { get; set; }

        public bool IsRequired { get; set; }

        public int DataTypeId { get; set; }

        public string CreatedBy { get; set; }
    }
}
