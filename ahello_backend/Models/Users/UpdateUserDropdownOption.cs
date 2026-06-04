namespace ahello_backend.Models.Users
{
    public class UpdateUserDropdownOption
    {
        public int UserDropDownId { get; set; }

        public string OptionValue { get; set; }

        public string OptionLabel { get; set; }

        public bool IsActive { get; set; }

        public string ModifiedBy { get; set; }
    }
}
