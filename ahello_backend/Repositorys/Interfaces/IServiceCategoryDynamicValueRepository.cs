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

        Task<IEnumerable<ServiceCategoryDynamicGetResponse>> GetAllAsync();

        Task<ServiceCategoryDynamicGetResponse> GetByIdAsync(
            int serviceCategoryId);
        Task<bool> UpdateStatusAsync(
        int serviceCategoryId,ServiceCategoryStatusUpdate model);
    }
}
