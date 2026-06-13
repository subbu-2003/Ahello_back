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

        public async Task<IEnumerable<ServiceCategoryDynamicGetResponse>>
            GetAllAsync()
        {
            return await _repository.GetAllAsync();
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
