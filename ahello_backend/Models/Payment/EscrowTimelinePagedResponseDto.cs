namespace ahello_backend.Models.Payment
{
    public class EscrowTimelinePagedResponseDto
    {
        public IEnumerable<EscrowTimelineResponseDto> Data { get; set; } = new List<EscrowTimelineResponseDto>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }
}
