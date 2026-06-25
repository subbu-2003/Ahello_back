using ahello_backend.Models.DynamicSearch;

namespace ahello_backend.Services.Interfaces
{
    public interface IFieldSearchService
    {
        Task<IEnumerable<DynamicFieldSearch>>
            SearchAsync(int userId, string keyword);
    }
}
