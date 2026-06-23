using ahello_backend.Models.Pagination;
using ahello_backend.Models.Service;
using ahello_backend.Models.Servicecategory;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class ServiceCategoryDynamicValueService
        : IServiceCategoryDynamicValueService
    {
        private readonly
            IServiceCategoryDynamicValueRepository _repository;

        public ServiceCategoryDynamicValueService(
            IServiceCategoryDynamicValueRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(
            ServiceCategoryDynamicPost model)
        {
            return await _repository.CreateAsync(model);
        }

        public async Task<bool> UpdateAsync(
            int serviceCategoryId,
            ServiceCategoryDynamicPut model)
        {
            return await _repository.UpdateAsync(
                serviceCategoryId,
                model);
        }

        public async Task<bool> DeleteAsync(
            int serviceCategoryId)
        {
            return await _repository.DeleteAsync(
                serviceCategoryId);
        }
        public async Task<PagedResult<ServiceCategoryDynamicGetResponse>>GetAllWithStatusAsync(
        int pageNumber,
        int pageSize,
        string? search,
        DateTime? createdDate,
        bool? isActive)
        {
            return await _repository.GetAllWithStatusAsync(
                pageNumber,
                pageSize,
                search,
                createdDate,
                isActive);
        }
        public async Task<PagedResult<ServiceCategoryDynamicGetResponse>> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? search,
        DateTime? createdDate)
        {
            return await _repository.GetAllAsync(
                pageNumber,
                pageSize,
                search,
                createdDate);
        }

        public async Task<ServiceCategoryDynamicGetResponse>
            GetByIdAsync(int serviceCategoryId)
        {
            return await _repository.GetByIdAsync(
                serviceCategoryId);
        }
        public async Task<bool> UpdateStatusAsync(
        int serviceCategoryId,
        ServiceCategoryStatusUpdate model)
        {
            return await _repository.UpdateStatusAsync(
                serviceCategoryId,
                model);
        }
    }
}
