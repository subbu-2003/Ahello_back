namespace ahello_backend.Models.DigitalBook
{
    public class DigitalBookApprovalRequest
    {
        public int AdminId { get; set; }

        public DigitalBookApprovalStatus ApprovalStatus { get; set; }

        public string? RejectionReason { get; set; }
    }

    public enum DigitalBookApprovalStatus
    {
        Approved,
        Rejected
    }
}
