namespace ahello_backend.Models.Service
{
    public class ServiceDropDownOptionResponse
    {
        public int ServiceDropDownId { get; set; }

        public string OptionValue { get; set; }

        public string OptionLabel { get; set; }

        public bool IsActive { get; set; }
    }
}
