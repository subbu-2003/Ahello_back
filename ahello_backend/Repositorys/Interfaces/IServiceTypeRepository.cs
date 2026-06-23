using ahello_backend.Models.Pagination;
using ahello_backend.Models.Servicetype;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IServiceTypeRepository
    {
        Task<IEnumerable<ServiceType>> GetAllAsync();
        Task<PagedResult<ServiceType>> GetPagedAsync(int pageNumber, int pageSize,
        string? search,DateTime? createdDate, bool? isActive = null);
        Task<ServiceType> GetByIdAsync(int serviceTypeId);

        Task<int> CreateAsync(ServiceTypePost model);

        Task<bool> UpdateAsync(
            int serviceTypeId,
            ServiceTypePut model);

        Task<bool> DeleteAsync(int serviceTypeId);
        Task<bool> UpdateStatusAsync(int serviceTypeId,ServiceTypeStatusUpdate model);
    }
}
