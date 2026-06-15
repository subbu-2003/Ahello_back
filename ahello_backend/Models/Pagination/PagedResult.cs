namespace ahello_backend.Models.Pagination
{
    public class PagedResult<T>
    {
        public int TotalCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public IEnumerable<T> Details { get; set; }

        public static PagedResult<T> Empty()
        {
            return new PagedResult<T>
            {
                TotalCount = 0,
                PageNumber = 1,
                PageSize = 10,
                Details = new List<T>()
            };
        }
    }
}
