namespace ahello_backend.Models.Service
{
    public class UpdateServiceField
    {
        public int ServiceFieldId { get; set; }

        public int ServiceId { get; set; }

        public string FieldName { get; set; }

        public string FieldCode { get; set; }

        public string Placeholder { get; set; }

        public bool IsRequired { get; set; }

        public bool IsActive { get; set; }

        public int DataTypeId { get; set; }

        public string ModifiedBy { get; set; }
    }
}
