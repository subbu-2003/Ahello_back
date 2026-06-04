namespace ahello_backend.Models.Users
{
    public class UpdateUserField
    {
        public int UserFieldId { get; set; }

        public int UserId { get; set; }

        public string FieldName { get; set; }

        public string FieldCode { get; set; }

        public string Placeholder { get; set; }

        public bool IsRequired { get; set; }

        public bool IsActive { get; set; }

        public int DataTypeId { get; set; }

        public string ModifiedBy { get; set; }
    }
}
