using ahello_backend.Models.User;

namespace ahello_backend.Models.Users
{
    public class UserDynamicPaginationResponse
    {
        public int TotalCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public List<UserDynamicGetResponse> Data { get; set; }
    }
}
