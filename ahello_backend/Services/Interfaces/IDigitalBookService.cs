using ahello_backend.Models.DigitalBook;

namespace ahello_backend.Services.Interfaces
{
    public interface IDigitalBookService
    {
        Task<int> CreateAsync(DigitalBook digitalBook);

        Task<bool> UpdateAsync(DigitalBook digitalBook);

        Task<IEnumerable<DigitalBook>> GetAllAsync();

        Task<IEnumerable<DigitalBook>> GetByUserIdAsync(int userId);

        Task<bool> DeactivateAsync(
            int digitalBookId,
            int modifiedBy);

        Task<IEnumerable<DigitalBook>> GetBySlugAsync(string slug);
        Task<DigitalBookAdminResponse> GetAdminDigitalBooksAsync(
          string? search,
          string? approvalStatus,
          int pageNumber,
          int pageSize);
        Task<IEnumerable<DigitalBook>> GetPendingAsync();

        Task<bool> UpdateApprovalStatusAsync(
           int digitalBookId,
           int adminId,
           DigitalBookApprovalStatus approvalStatus,
           string? rejectionReason);

        Task<bool> PublishAsync(
            int digitalBookId,
            int userId);
    }
}
