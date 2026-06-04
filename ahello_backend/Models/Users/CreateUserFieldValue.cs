namespace ahello_backend.Models.Users
{
    public class CreateUserFieldValue
    {
        public int UserId { get; set; }

        public string FieldCode { get; set; }

        public int UserFieldId { get; set; }

        public string? FieldValue { get; set; }

        public string CreatedBy { get; set; }
    }
}
