using ahello_backend.Models.Meeting;
using ahello_backend.Models.Pagination;
using ahello_backend.Repositorys.Classes;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class MeetingService : IMeetingService
    {
        private readonly IMeetingRepository _repo;

        public MeetingService(IMeetingRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Meeting>> GetAllAsync()
            => await _repo.GetAllAsync();

        public async Task<Meeting> GetByIdAsync(int meetingId)
            => await _repo.GetByIdAsync(meetingId);

        public async Task<IEnumerable<Meeting>> GetByBookingIdAsync(int bookingId)
            => await _repo.GetByBookingIdAsync(bookingId);
        public async Task<Meeting> GetByRoomNameAsync(string roomName)
        {
            return await _repo.GetByRoomNameAsync(roomName);
        }

        public async Task<PagedResult<Meeting>> GetByUserIdAsync( int userId,int pageNumber,int pageSize, string? status,DateTime? createdDate)
            => await _repo.GetByUserIdAsync(userId,pageNumber,pageSize, status,
        createdDate);

        public async Task<int> CreateAsync(MeetingPost model)
            => await _repo.CreateAsync(model);

        public async Task<bool> UpdateAsync(MeetingPut model)
            => await _repo.UpdateAsync(model);

        public async Task<bool> DeleteAsync(int meetingId)
            => await _repo.DeleteAsync(meetingId);
        public async Task<bool> SendMeetingReminderAsync()
        {
            // ✅ Only fetch meetings that actually need a reminder — DB-filtered
            var meetings = await _repo.GetPendingRemindersAsync();
            var now = DateTime.Now;

            foreach (var meeting in meetings)
            {
                int minutesLeft = (int)Math.Round((meeting.StartTime - now).TotalMinutes);

                // ✅ Mark FIRST — prevents double-send if job ticks again before email completes
                await _repo.UpdateReminderSentAsync(meeting.MeetingId);

                await _repo.SendMeetingReminderMailAsync(meeting, minutesLeft);
            }

            return true;
        }

    }
}