namespace ahello_backend.Models.Pagination
{
    public class PagedResult<T>
    {
        public int TotalCount { get; set; }

        public IEnumerable<T> Details { get; set; }

        public static PagedResult<T> Empty()
        {
            return new PagedResult<T>
            {
                TotalCount = 0,
                Details = new List<T>()
            };
        }
    }
}
