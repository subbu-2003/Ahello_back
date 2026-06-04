namespace ahello_backend.Models.Service
{
    public class ServiceBasicField
    {
        public string FieldCode { get; set; }

        public string Label { get; set; }

        public string DataType { get; set; }

        public bool IsRequired { get; set; }

        public List<ServiceDropdownOption>? Options { get; set; }
    }
}
