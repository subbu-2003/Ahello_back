namespace ahello_backend.Models.Service
{
    public class ServiceField
    {
        public int ServiceFieldId { get; set; }

        public int UserId { get; set; }

        public string FieldName { get; set; }

        public string FieldCode { get; set; }

        public string Placeholder { get; set; }

        public bool IsRequired { get; set; }

        public bool IsActive { get; set; }

        public int DataTypeId { get; set; }
        public string? DataTypeName { get; set; }
        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public List<ServiceDropdownOptionModel>
            DropdownOptions
        { get; set; }
                = new List<ServiceDropdownOptionModel>();
    }
}
