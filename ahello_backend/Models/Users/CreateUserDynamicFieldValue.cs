namespace ahello_backend.Models.Users
{
    public class CreateUserDynamicFieldValue
    {
        public int? UserFieldId { get; set; }

        public string? FieldValue { get; set; }

        public List<UserDropdownOptionPut>? DropDownOptions { get; set; }
    }
}
