namespace ahello_backend.Models.Users
{
    public class UserDropdownOptionPut
    {
        public int? UserDropDownId { get; set; }
        public string? OptionValue { get; set; }

        public string? OptionLabel { get; set; }

        public bool? IsActive { get; set; }
    }
}
