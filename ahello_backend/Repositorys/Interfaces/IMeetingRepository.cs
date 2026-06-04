using ahello_backend.Models.Meeting;
using ahello_backend.Models.Pagination;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IMeetingRepository
    {
        Task<IEnumerable<Meeting>> GetAllAsync();

        Task<Meeting> GetByIdAsync(int meetingId);

        Task<IEnumerable<Meeting>> GetByBookingIdAsync(int bookingId);
        Task<Meeting> GetByRoomNameAsync(string roomName);
        Task<PagedResult<Meeting>> GetByUserIdAsync(int userId,int pageNumber,int pageSize);

        Task<int> CreateAsync(MeetingPost model);

        Task<bool> UpdateAsync(MeetingPut model);

        Task<bool> DeleteAsync(int meetingId);
        Task<bool> SendMeetingReminderMailAsync(Meeting meeting);

        Task UpdateReminderSentAsync(int meetingId);
    }
}
