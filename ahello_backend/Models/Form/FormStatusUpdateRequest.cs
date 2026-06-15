namespace ahello_backend.Models.Form
{
    public class FormStatusUpdateRequest
    {
        public int FormId { get; set; }
        public bool IsActive { get; set; }
        public int ModifiedBy { get; set; }
    }
}
