namespace ahello_backend.Models.Users
{
    public class UserDynamicFieldFormResponse
    {
        public List<UserBasicField> BasicFields { get; set; }

        public List<UserDynamicField> DynamicFields { get; set; }
    }
}
