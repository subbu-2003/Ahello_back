namespace ahello_backend.Models.Category
{
    public class Category
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; }

        public DateTime ModifiedAt { get; set; }

        public string ModifiedBy { get; set; }
    }
}
