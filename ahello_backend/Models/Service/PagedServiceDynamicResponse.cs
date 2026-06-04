namespace ahello_backend.Models.Service
{
    public class PagedServiceDynamicResponse
    {
        public int TotalRecords { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public IEnumerable<ServiceDynamicGetResponse> Data { get; set; }
    }
}
