using ahello_backend.Models.Service;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IServiceDynamicRepository
    {
        Task<IEnumerable<ServiceDynamicGetResponse>> GetAllAsync();

        Task<ServiceDynamicGetResponse> GetByIdAsync(int serviceId);
        // IServiceDynamicRepository.cs

        Task<PagedServiceDynamicResponse> GetByUserIdPagedAsync(
        int userId,
        int pageNumber,
        int pageSize,
        string? search = null,
        string? serviceCategoryName = null,
        string? status = null,
        bool? isActive = null);
        Task<int> CreateAsync(ServiceDynamicPost model);

        Task<bool> UpdateAsync(int serviceId, ServiceDynamicPut model);

        Task<bool> DeleteAsync(int serviceId);

        Task<PagedServiceDynamicResponse> GetAllPagedAsync(
            int pageNumber,
            int pageSize,
            string? search = null);
        Task<bool> UpdateServiceIsActiveAsync( int serviceId,ServiceIsActivePut model);

    }
}
