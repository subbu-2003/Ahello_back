using ahello_backend.Models.Service;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IServiceFieldValueRepository
    {
        Task<int> CreateAsync(CreateServiceFieldValue model);

        Task<bool> UpdateAsync(UpdateServiceFieldValue model);

        Task<IEnumerable<ServiceFieldValue>> GetByServiceAsync(int serviceId);

        Task<ServiceFieldValue> GetByIdAsync(int serviceFieldValueId);
    }
}
