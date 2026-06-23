using ahello_backend.Models.Pagination;
using ahello_backend.Models.Servicecategory;

namespace ahello_backend.Services.Interfaces
{
    public interface IServiceCategoryDynamicValueService
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

        Task<PagedResult<ServiceCategoryDynamicGetResponse>>GetAllWithStatusAsync(
        int pageNumber,
        int pageSize,
        string? search,
        DateTime? createdDate,
        bool? isActive);

        Task<ServiceCategoryDynamicGetResponse>
            GetByIdAsync(int serviceCategoryId);
        Task<bool> UpdateStatusAsync( int serviceCategoryId, ServiceCategoryStatusUpdate model);
    }
}
