namespace ahello_backend.Models.Users
{
    public class UserServiceResponse
    {
        public int UserId { get; set; }

        public string FullName { get; set; }
        public string ProfileUrl { get; set; }
        public string? Slug { get; set; }
        public int? CategoryId { get; set; }

        public string CategoryName { get; set; }

        public List<UserServiceItem> Services { get; set; }
            = new List<UserServiceItem>();
    }
}
