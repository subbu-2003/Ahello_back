namespace ahello_backend.Models.Category
{
    public class PagedCategoryDynamicResponse
    {
        public int TotalRecords { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public IEnumerable<CategoryDynamicGetResponse> Data { get; set; }
    }
}
