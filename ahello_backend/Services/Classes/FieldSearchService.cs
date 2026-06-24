using ahello_backend.Models.DynamicSearch;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class FieldSearchService : IFieldSearchService
    {
        private readonly IFieldSearchRepository _repo;

        public FieldSearchService(IFieldSearchRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<DynamicFieldSearch>>
            SearchAsync(int userId, string keyword)
        {
            if (userId <= 0 ||
                string.IsNullOrWhiteSpace(keyword))
            {
                return new List<DynamicFieldSearch>();
            }

            return await _repo.SearchFieldAsync(
                userId,
                keyword);
        }
    }
}
