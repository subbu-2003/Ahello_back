namespace ahello_backend.Models.Users
{
    public class UserSearchDto
    {
        public string FullName { get; set; }

        public string CategoryName { get; set; }

        public string ServiceTypeName { get; set; }

        public string ServiceCategoryName { get; set; }
        public string ServiceTitle { get; set; }

        public decimal Price { get; set; }
    }
}
