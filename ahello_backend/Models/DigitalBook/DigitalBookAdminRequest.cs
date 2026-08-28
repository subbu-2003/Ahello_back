namespace ahello_backend.Models.DigitalBook
{
    public class DigitalBookAdminRequest
    {
        public string? Search { get; set; }

        public DigitalBookApprovalStatus? ApprovalStatus { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
