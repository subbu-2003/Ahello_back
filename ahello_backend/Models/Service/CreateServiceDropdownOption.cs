namespace ahello_backend.Models.Service
{
    public class CreateServiceDropdownOption
    {
        public int ServiceFieldId { get; set; }

        public int ServiceId { get; set; }

        public string CreatedBy { get; set; }

        public List<ServiceDropdownOptionItem> Options { get; set; }
            = new List<ServiceDropdownOptionItem>();
    }
}
