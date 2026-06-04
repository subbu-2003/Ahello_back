namespace ahello_backend.Models.Service
{
    public class ServiceDynamicFieldPut
    {
        public int ServiceFieldId { get; set; }

        public string FieldValue { get; set; }

        public List<ServiceDropDownOptionPut> DropDownOptions { get; set; }
            = new List<ServiceDropDownOptionPut>();
    }
}
