using ahello_backend.Models.Bookings;
using ahello_backend.Models.Pagination;
using ahello_backend.Models.Reschedulerequest;

namespace ahello_backend.Services.Interfaces
{
    public interface IBookingService
    {
        Task<int> CreateAsync(BookingPost model);
        Task<int> RescheduleAsync(
        int oldBookingId,
        DateTime newDate,
        TimeSpan newStart,
        TimeSpan newEnd,
        int slotId,
        string rescheduledBy,
        string reason);

        Task<IEnumerable<BookingRead>> GetAllAsync();

        Task<BookingRead> GetByIdAsync(int bookingId);

        Task<bool> UpdateAsync(BookingPut model);

        Task<bool> DeleteAsync(int bookingId);
        Task<PagedResult<BookingRead>> GetByUserIdAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? search, string? status,
            DateTime? scheduleDate);
        Task<PagedResult<BookingRead>> GetByClientIdAsync(
            int clientId,
            int pageNumber,
            int pageSize,
            string? search, string? status,
            DateTime? scheduleDate);

        Task<BookingModalGet>GetBookingModal( int serviceId);
        Task<List<ServiceWiseClientGet>> GetClientsServiceWiseAsync(
        int userId,
        int pageNumber,
        int pageSize,
        string? search,
        string? bookingStatus,
        DateTime? lastBookingDate);
        Task<int> CreateRescheduleRequestAsync(
        RescheduleRequestPost model);

        Task<PagedResult<RescheduleRequestRead>>GetRescheduleRequestsByUserIdAsync(
        int userId,
        int pageNumber,
        int pageSize,
        DateTime? requestedDate);

        Task<RescheduleRequestRead?>
            GetRescheduleRequestByIdAsync(int requestId);

        Task<bool> UpdateRescheduleRequestStatusAsync(
            int requestId,
            string status,
            string modifiedBy);
    }
}
