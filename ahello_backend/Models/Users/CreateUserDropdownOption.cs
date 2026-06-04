namespace ahello_backend.Models.Users
{
    public class CreateUserDropdownOption
    {
        public int UserFieldId { get; set; }

        public int? UserId { get; set; }

        public string CreatedBy { get; set; }

        public List<UserDropdownOptionItem> Options { get; set; }
    }

    public class UserDropdownOptionItem
    {
        public string? OptionValue { get; set; }

        public string? OptionLabel { get; set; }

        public bool? IsActive { get; set; }
    }
}
