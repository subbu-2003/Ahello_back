using ahello_backend.Models.Payment;
using ahello_backend.Models.Payments;
namespace ahello_backend.Repositorys.Interfaces
{
    public interface IExpertPayoutRepository
    {
        Task<ExpertPayoutAccount?> GetByUserIdAsync(int userId);
        Task<int> InsertAsync(ExpertPayoutAccount account);

        Task UpdateAccountCreatedAsync(int userId, string accountId, string responseJson);
        Task UpdateStakeholderCreatedAsync(int userId, string stakeholderId, string responseJson);
        Task UpdateProductRequestedAsync(int userId, string productId, string responseJson);
        Task UpdateProductActivatedAsync(int userId, string responseJson);

        Task SetFailedAsync(int userId, string failureReason);
    }
}
