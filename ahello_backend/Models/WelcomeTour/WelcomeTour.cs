namespace ahello_backend.Models.WelcomeTour
{
    public class WelcomeTour
    {
        public int WelcomeTourId { get; set; }
        public int UserId { get; set; }
        public string? ItemType { get; set; }
        public string? ItemKey { get; set; }
        public bool IsActive { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
