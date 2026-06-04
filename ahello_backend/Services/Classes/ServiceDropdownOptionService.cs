using ahello_backend.Models.Service;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class ServiceDropdownOptionService
        : IServiceDropdownOptionService
    {
        private readonly IServiceDropdownOptionRepository
            _repository;

        public ServiceDropdownOptionService(
            IServiceDropdownOptionRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(
            CreateServiceDropdownOption model)
        {
            return await _repository.CreateAsync(model);
        }

        public async Task<bool> UpdateAsync(
            UpdateServiceDropdownOption model)
        {
            return await _repository.UpdateAsync(model);
        }

        public async Task<GetServiceDropdownOption>
            GetByIdAsync(
                int optionId)
        {
            return await _repository.GetByIdAsync(
                optionId);
        }

        public async Task<IEnumerable<GetServiceDropdownOption>>
            GetByFieldIdAsync(
                int serviceFieldId,
                int? serviceId)
        {
            return await _repository.GetByFieldIdAsync(
                serviceFieldId,
                serviceId);
        }
    }
}
