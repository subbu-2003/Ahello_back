namespace ahello_backend.Models.Service
{
    public class UpdateServiceFieldValue
    {
        public int ServiceFieldValueId { get; set; }

        public string? FieldValue { get; set; }

        public string ModifiedBy { get; set; }
    }
}
