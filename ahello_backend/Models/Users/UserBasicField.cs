namespace ahello_backend.Models.Users
{
    public class UserBasicField
    {
        public string FieldCode { get; set; }

        public string Label { get; set; }

        public string DataType { get; set; }

        public bool IsRequired { get; set; }

        public List<UserDropdownOption>? Options { get; set; }
    }
}
