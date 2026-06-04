using ahello_backend.Models.Bookings;
using ahello_backend.Models.Pagination;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IBookingRepository
    {
        Task<int> CreateAsync(BookingPost model);

        Task<IEnumerable<BookingRead>> GetAllAsync();

        Task<BookingRead> GetByIdAsync(int bookingId);

        Task<bool> UpdateAsync(BookingPut model);

        Task<bool> DeleteAsync(int bookingId);
        Task<PagedResult<BookingRead>> GetByUserIdAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? search);
        Task<PagedResult<BookingRead>> GetByClientIdAsync(
            int clientId,
            int pageNumber,
            int pageSize,
            string? search);

        Task<BookingModalGet> GetBookingModal(int serviceId);
    }

}
