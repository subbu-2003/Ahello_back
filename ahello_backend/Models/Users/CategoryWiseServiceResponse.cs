namespace ahello_backend.Models.Users
{
    public class CategoryWiseServiceResponse
    {
        public int? ServiceCategoryId { get; set; }

        public string? ServiceCategoryName { get; set; }

        public List<CategoryWiseService> Services { get; set; } = new();
    }
}
