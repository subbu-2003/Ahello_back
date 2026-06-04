using ahello_backend.Models.Service;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IServiceDropdownOptionRepository
    {
        Task<int> CreateAsync(
            CreateServiceDropdownOption model);

        Task<bool> UpdateAsync(
            UpdateServiceDropdownOption model);

        Task<GetServiceDropdownOption> GetByIdAsync(
            int optionId);

        Task<IEnumerable<GetServiceDropdownOption>>
            GetByFieldIdAsync(
                int serviceFieldId,
                int? serviceId);
    }
}
