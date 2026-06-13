namespace ahello_backend.Models.Category
{
    public class CategoryStatusUpdateRequest
    {
        public bool IsActive { get; set; }
        public string ModifiedBy { get; set; }
    }
}
