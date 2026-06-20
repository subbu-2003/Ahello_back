namespace ahello_backend.Models.Service
{
    public class CreateServiceDynamicFieldValue
    {
        public int ServiceFieldId { get; set; }

        public string? FieldValue { get; set; }
        public List<ServiceDropDownOptionPut>? DropDownOptions { get; set; }
    }
}
