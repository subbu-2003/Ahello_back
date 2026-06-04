namespace ahello_backend.Models.Service
{
    public class ServiceDynamicField
    {
        public int ServiceFieldId { get; set; }

        public string FieldName { get; set; }

        public string FieldCode { get; set; }

        public string Placeholder { get; set; }

        public int DataTypeId { get; set; }

        public bool IsRequired { get; set; }
    }
}
