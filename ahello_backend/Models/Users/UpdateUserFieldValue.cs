namespace ahello_backend.Models.Users
{

    public class UpdateUserFieldValue
    {
        public int UserFieldValueId { get; set; }

        public string FieldValue { get; set; }

        public string ModifiedBy { get; set; }
    }
}
