namespace ahello_backend.Models.Form
{
    public class FormCreate
    {
        public int UserId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public int? CreatedBy { get; set; }
    }
}
