namespace ahello_backend.Models.Bookings
{
    public class ServiceWiseClientGet
    {
        public int ServiceId { get; set; }
        public string ServiceTitle { get; set; }
        public int TotalClients { get; set; }

        public List<ServiceClientGet> Clients { get; set; } = new();
    }
}
