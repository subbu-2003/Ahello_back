using ahello_backend.Models.Meeting;
using ahello_backend.Models.Pagination;

namespace ahello_backend.Services.Interfaces
{
    public interface IMeetingService
    {
        Task<IEnumerable<Meeting>> GetAllAsync();

        Task<Meeting> GetByIdAsync(int meetingId);

        Task<IEnumerable<Meeting>> GetByBookingIdAsync(int bookingId);
        Task<Meeting> GetByRoomNameAsync(string roomName);

        Task<PagedResult<Meeting>> GetByUserIdAsync(int userId,int pageNumber,int pageSize, string? status, DateTime? createdDate);

        Task<int> CreateAsync(MeetingPost model);

        Task<bool> UpdateAsync(MeetingPut model);

        Task<bool> DeleteAsync(int meetingId);
        Task<bool> SendMeetingReminderAsync();
    }
}
