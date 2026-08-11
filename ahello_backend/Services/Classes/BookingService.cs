using ahello_backend.Models.Bookings;
using ahello_backend.Models.Pagination;
using ahello_backend.Models.Reschedulerequest;
using ahello_backend.Repositorys.Classes;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _repository;

        public BookingService(IBookingRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(BookingPost model)
        {
            return await _repository.CreateAsync(model);
        }

        public async Task<int> RescheduleAsync(
        int oldBookingId,
        DateTime newDate,
        TimeSpan newStart,
        TimeSpan newEnd,
        int slotId,
        string rescheduledBy,
        string reason)
        {
            return await _repository.RescheduleAsync(
                oldBookingId,
                newDate,
                newStart,
                newEnd,
                slotId,
                rescheduledBy,
                reason);
        }

        public async Task<IEnumerable<BookingRead>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<BookingRead> GetByIdAsync(int bookingId)
        {
            return await _repository.GetByIdAsync(bookingId);
        }

        public async Task<bool> UpdateAsync(BookingPut model)
        {
            return await _repository.UpdateAsync(model);
        }

        public async Task<bool> DeleteAsync(int bookingId)
        {
            return await _repository.DeleteAsync(bookingId);
        }
        public async Task<PagedResult<BookingRead>> GetByUserIdAsync(
            int userId,
            int pageNumber,
            int pageSize,
            string? search, string? status,
            DateTime? scheduleDate)
        {
            return await _repository.GetByUserIdAsync(
                userId,
                pageNumber,
                pageSize,
                search, status,
                scheduleDate);
        }
        public async Task<PagedResult<BookingRead>> GetByClientIdAsync(
            int clientId,
            int pageNumber,
            int pageSize,
            string? search, string? status,
            DateTime? scheduleDate)
        {
            return await _repository.GetByClientIdAsync(
                clientId,
                pageNumber,
                pageSize,
                search, status,
                scheduleDate);
        }
        public async Task<BookingModalGet>GetBookingModal(int serviceId
    )
        {
            return await _repository
                .GetBookingModal(
                    serviceId
                );
        }

        public async Task<List<ServiceWiseClientGet>> GetClientsServiceWiseAsync(
        int userId,
        int pageNumber,
        int pageSize,
        string? search,
        string? bookingStatus,
        DateTime? lastBookingDate)
        {
            return await _repository.GetClientsServiceWiseAsync(
                userId,
                pageNumber,
                pageSize,
                search,
                bookingStatus,
                lastBookingDate);
        }
        public async Task<int> CreateRescheduleRequestAsync(
    RescheduleRequestPost model)
        {
            return await _repository .CreateRescheduleRequestAsync(model);
        }

        public async Task<IEnumerable<RescheduleRequestRead>>
            GetRescheduleRequestsByUserIdAsync(int userId)
        {
            return await _repository.GetRescheduleRequestsByUserIdAsync(userId);
        }

        public async Task<RescheduleRequestRead?>
            GetRescheduleRequestByIdAsync(int requestId)
        {
            return await _repository.GetRescheduleRequestByIdAsync(requestId);
        }

        public async Task<bool> UpdateRescheduleRequestStatusAsync(
            int requestId,
            string status,
            string modifiedBy)
        {
            return await _repository
                .UpdateRescheduleRequestStatusAsync(
                    requestId,
                    status,
                    modifiedBy);
        }
    }
}