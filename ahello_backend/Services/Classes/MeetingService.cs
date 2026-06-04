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

        public async Task<PagedResult<Meeting>> GetByUserIdAsync( int userId,int pageNumber,int pageSize)
            => await _repo.GetByUserIdAsync(userId,pageNumber,pageSize);

        public async Task<int> CreateAsync(MeetingPost model)
            => await _repo.CreateAsync(model);

        public async Task<bool> UpdateAsync(MeetingPut model)
            => await _repo.UpdateAsync(model);

        public async Task<bool> DeleteAsync(int meetingId)
            => await _repo.DeleteAsync(meetingId);
        public async Task<bool> SendMeetingReminderAsync()
        {
            var meetings = await _repo.GetAllAsync();

            var now = DateTime.Now;

            foreach (var meeting in meetings)
            {
                // ✅ Skip completed meetings
                if (meeting.Status == "Completed")
                    continue;

                // ✅ Skip already sent reminders
                if (meeting.ReminderSent)
                    continue;

                // ✅ Time window: 10 mins to 5 mins before start
                var fromTime = meeting.StartTime.AddMinutes(-10);
                var toTime = meeting.StartTime.AddMinutes(-5);

                if (now >= fromTime && now <= toTime)
                {
                    var sent = await _repo.SendMeetingReminderMailAsync(meeting);

                    if (sent)
                    {
                        await _repo.UpdateReminderSentAsync(meeting.MeetingId);
                    }
                }
            }

            return true;
        }
    }
}