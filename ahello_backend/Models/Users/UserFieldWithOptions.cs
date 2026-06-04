namespace ahello_backend.Models.Users
{
    public class UserFieldWithOptions
    {
        public int UserFieldId { get; set; }

        public int? UserId { get; set; }

        public string FieldName { get; set; }

        public string FieldCode { get; set; }

        public string Placeholder { get; set; }

        public bool IsRequired { get; set; }

        public bool IsActive { get; set; }

        public int DataTypeId { get; set; }

        public List<UserDropdownOptionItemRead> Options { get; set; }
            = new();
    }

    public class UserDropdownOptionItemRead
    {
        public int UserDropDownId { get; set; }

        public string OptionValue { get; set; }

        public string OptionLabel { get; set; }

        public bool IsActive { get; set; }
    }
}
