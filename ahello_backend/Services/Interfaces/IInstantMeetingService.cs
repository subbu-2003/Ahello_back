using ahello_backend.Models.Meeting;

namespace ahello_backend.Services.Interfaces
{
    public interface IInstantMeetingService
    {
        Task<int> CreateAsync(InstantMeeting model);

        Task<InstantMeeting?> GetByRoomNameAsync(string roomName);
    }
}