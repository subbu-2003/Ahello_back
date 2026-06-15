namespace ahello_backend.Models.Form
{
    public class FormSearchRequest
    {
        public int UserId { get; set; }

        public string? SearchText { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? Date { get; set; }
    }
}
