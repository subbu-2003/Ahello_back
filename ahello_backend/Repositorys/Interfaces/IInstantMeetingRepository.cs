using ahello_backend.Models.Meeting;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IInstantMeetingRepository
    {
        Task<int> CreateAsync(InstantMeeting model);

        Task<InstantMeeting?> GetByRoomNameAsync(string roomName);
    }
}