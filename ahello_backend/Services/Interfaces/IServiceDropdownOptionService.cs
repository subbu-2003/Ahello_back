using ahello_backend.Models.Service;

namespace ahello_backend.Services.Interfaces
{
    public interface IServiceDropdownOptionService
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
