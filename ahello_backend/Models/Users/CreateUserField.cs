namespace ahello_backend.Models.Users
{
    public class CreateUserField
    {
        public int UserId { get; set; }

        public string FieldName { get; set; }

        public string FieldCode { get; set; }

        public string Placeholder { get; set; }

        public bool IsRequired { get; set; }

        public int DataTypeId { get; set; }

        public string CreatedBy { get; set; }
    }
}
