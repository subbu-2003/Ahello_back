using ahello_backend.Models.Bookings;
using ahello_backend.Models.Pagination;
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
            string? search)
        {
            return await _repository.GetByUserIdAsync(
                userId,
                pageNumber,
                pageSize,
                search);
        }
        public async Task<PagedResult<BookingRead>> GetByClientIdAsync(
            int clientId,
            int pageNumber,
            int pageSize,
            string? search)
        {
            return await _repository.GetByClientIdAsync(
                clientId,
                pageNumber,
                pageSize,
                search);
        }
        public async Task<BookingModalGet>
    GetBookingModal(
        int serviceId
    )
        {
            return await _repository
                .GetBookingModal(
                    serviceId
                );
        }
    }
}