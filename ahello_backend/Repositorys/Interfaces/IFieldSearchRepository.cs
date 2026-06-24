using ahello_backend.Models.DynamicSearch;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IFieldSearchRepository
    {
        Task<IEnumerable<DynamicFieldSearch>> SearchFieldAsync(
            int userId,
            string keyword);
    }
}
