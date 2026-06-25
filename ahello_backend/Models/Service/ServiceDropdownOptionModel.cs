namespace ahello_backend.Models.Service
{
    public class ServiceDropdownOptionModel
    {
        public int ServiceDropDownId { get; set; }

        public int ServiceFieldId { get; set; }

        public int UserId { get; set; }

        public string OptionValue { get; set; }

        public string OptionLabel { get; set; }

        public bool IsActive { get; set; }
    }
}
