namespace ahello_backend.Models.Users
{
    public class UserFieldValue
    {
        public int UserFieldValueId { get; set; }

        public int UserId { get; set; }

        public string FieldCode { get; set; }

        public int UserFieldId { get; set; }

        public string FieldValue { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public string ModifiedBy { get; set; }

        public DateTime ModifiedAt { get; set; }
    }
}
