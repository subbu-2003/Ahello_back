namespace ahello_backend.Models.Users
{
    public class GetUserDropdownOption
    {
        public int UserDropDownId { get; set; }

        public int UserFieldId { get; set; }

        public int? UserId { get; set; }

        public string OptionValue { get; set; }

        public string OptionLabel { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string ModifiedBy { get; set; }
    }
}
