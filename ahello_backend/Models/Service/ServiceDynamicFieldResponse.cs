namespace ahello_backend.Models.Service
{
    public class ServiceDynamicFieldResponse
    {
        public int ServiceFieldId { get; set; }
        public int ServiceId { get; set; }
        public int UserId { get; set; }
        public string FieldName { get; set; }

        public string FieldCode { get; set; }

        public string? FieldValue { get; set; }
        public List<ServiceDropDownOptionResponse> DropDownOptions { get; set; }
      = new List<ServiceDropDownOptionResponse>();
    }
}
