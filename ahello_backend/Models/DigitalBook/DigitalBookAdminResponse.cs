namespace ahello_backend.Models.DigitalBook
{
    public class DigitalBookAdminResponse
    {
        public IEnumerable<DigitalBook> Data { get; set; }
            = new List<DigitalBook>();

        public int TotalRecords { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }
    }
}
