using ahello_backend.Models.Meeting;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IInstantMeetingRepository
    {
        Task<int> CreateAsync(InstantMeeting model);

        Task<InstantMeeting?> GetByRoomNameAsync(string roomName);

        Task<int> CreateJoinRequestAsync(InstantMeetingJoinRequest model);

        Task<IEnumerable<InstantMeetingJoinRequest>> GetWaitingUsersAsync(int instantMeetingId);

        Task<InstantMeetingJoinRequest?> GetJoinRequestByIdAsync(int requestId);

        Task<int> UpdateJoinRequestStatusAsync(int requestId, string status);

        Task<int> UpdateAllJoinRequestStatusAsync(int instantMeetingId, string status);
        Task<InstantMeetingJoinRequest?> GetByMeetingAndUserAsync(int instantMeetingId, int userId);
    }
}