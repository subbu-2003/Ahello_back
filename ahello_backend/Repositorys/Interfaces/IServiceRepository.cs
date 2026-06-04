using ahello_backend.Models.Service;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IServiceRepository
    {
               Task<IEnumerable<Service>> GetAllAsync();

       Task<Service> GetByIdAsync(int serviceId);

       Task<int> CreateAsync(ServicePost model);

       Task<bool> UpdateAsync(
           int serviceId,
           ServicePut model);

       Task<bool> DeleteAsync(int serviceId);

       Task<IEnumerable<Service>> GetByServiceCategoryIdAsync(int serviceCategoryId);

        Task<Service> GetById(int serviceId);
    }
}
