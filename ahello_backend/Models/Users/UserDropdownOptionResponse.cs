namespace ahello_backend.Models.Users
{
    public class UserDropdownOptionResponse
    {
        public int UserDropDownId { get; set; }

        public int UserFieldId { get; set; }

        public int UserId { get; set; }

        public string OptionValue { get; set; }

        public string OptionLabel { get; set; }

        public bool IsActive { get; set; }
    }
}
