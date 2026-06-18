namespace ahello_backend.Models.Users
{
    public class UserDynamicFieldPut
    {
        public int? UserFieldValueId { get; set; }
        public int UserFieldId { get; set; }

        public string? FieldValue { get; set; }

        public List<UserDropdownOptionPut> DropDownOptions { get; set; }
        = new();
    }
}
