namespace ahello_backend.Models.Service
{
    public class CreateServiceFieldValue
    {
        public int UserId { get; set; }

        public string FieldCode { get; set; }

        public int ServiceFieldId { get; set; }

        public string? FieldValue { get; set; }

        public string CreatedBy { get; set; }
    }
}
