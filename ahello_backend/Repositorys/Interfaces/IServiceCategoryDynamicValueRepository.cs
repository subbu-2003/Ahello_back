using ahello_backend.Models.Pagination;
using ahello_backend.Models.Service;
using ahello_backend.Models.Servicecategory;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IServiceCategoryDynamicValueRepository
    {
        Task<int> CreateAsync(ServiceCategoryDynamicPost model);

        Task<bool> UpdateAsync(
            int serviceCategoryId,
            ServiceCategoryDynamicPut model);

        Task<bool> DeleteAsync(int serviceCategoryId);

        Task<PagedResult<ServiceCategoryDynamicGetResponse>> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? search,
        DateTime? createdDate);
        Task<PagedResult<ServiceCategoryDynamicGetResponse>> GetAllWithStatusAsync(
        int pageNumber,int pageSize,string? search,DateTime? createdDate);
        Task<ServiceCategoryDynamicGetResponse> GetByIdAsync(
            int serviceCategoryId);
        Task<bool> UpdateStatusAsync(
        int serviceCategoryId,ServiceCategoryStatusUpdate model);
    }
}
