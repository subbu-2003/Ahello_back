namespace ahello_backend.Models.Service
{
    public class ServiceDynamicFieldFormResponse
    {
        public List<ServiceBasicField> BasicFields { get; set; }

        public List<ServiceDynamicField> DynamicFields { get; set; }
    }
}
