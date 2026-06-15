namespace ahello_backend.Models.Servicetype
{
    public class ServiceTypePost
    {
        public string ServiceTypeName { get; set; }
        public bool IsActive { get; set; } = true;
        public string CreatedBy { get; set; }
    }
}
