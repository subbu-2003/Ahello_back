namespace ahello_backend.Models.WelcomeTour
{
    public class UpdateWelcomeTourItemStatus
    {
        public string ItemKey { get; set; }
        public bool IsActive { get; set; }
        public int ModifiedBy { get; set; }
    }
}
