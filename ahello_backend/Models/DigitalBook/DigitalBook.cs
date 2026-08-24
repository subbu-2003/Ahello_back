namespace ahello_backend.Models.DigitalBook
{
    public class DigitalBook
    {
        public int DigitalBookId { get; set; }
        public int UserId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public string PreviewImage { get; set; }
        public string PdfFile { get; set; }

        public string Status { get; set; } = "Draft";

        public string ApprovalStatus { get; set; } = "Pending";

        public int? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public string? RejectionReason { get; set; }

        public decimal Price { get; set; }

        public bool IsActive { get; set; }

        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
