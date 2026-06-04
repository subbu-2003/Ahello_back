namespace ahello_backend.Models.Servicetype
{
    public class ServiceType
    {
        public int ServiceTypeId { get; set; }

        public string ServiceTypeName { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public string? ModifiedBy { get; set; }
    }
}
