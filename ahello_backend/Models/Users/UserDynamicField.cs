namespace ahello_backend.Models.Users
{
    public class UserDynamicField
    {
        public int UserFieldId { get; set; }

        public string FieldName { get; set; }

        public string FieldCode { get; set; }

        public string Placeholder { get; set; }

        public int DataTypeId { get; set; }

        public bool IsRequired { get; set; }

        public List<UserDropdownOption>? Options { get; set; }
    }
}
