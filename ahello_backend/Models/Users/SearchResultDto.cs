namespace ahello_backend.Models.Users
{
    public class SearchResultDto
    {
        public int TotalCount { get; set; }
        public List<UserSearchDto> Details { get; set; }
        public List<ExpertSearchDto> Experts { get; set; }
    }
}
