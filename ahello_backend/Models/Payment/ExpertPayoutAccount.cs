namespace ahello_backend.Models.Payments
{
    public class ExpertPayoutAccount
    {
        public int ExpertPayoutAccountId { get; set; }
        public int UserId { get; set; }

        public string? RazorpayAccountId { get; set; }
        public string? RazorpayStakeholderId { get; set; }
        public string? RazorpayProductId { get; set; }

        public string AccountStatus { get; set; } = "NOT_CREATED";

        public string? BusinessType { get; set; }
        public string? CustomerFacingBusinessName { get; set; }

        public string? RazorpayAccountResponseJson { get; set; }
        public string? RazorpayStakeholderResponseJson { get; set; }
        public string? RazorpayProductResponseJson { get; set; }

        public string? FailureReason { get; set; }

        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
