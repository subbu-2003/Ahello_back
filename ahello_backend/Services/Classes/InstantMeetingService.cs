using ahello_backend.Models.Meeting;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class InstantMeetingService : IInstantMeetingService
    {
        private readonly IInstantMeetingRepository _repository;

        public InstantMeetingService(
            IInstantMeetingRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(
            InstantMeeting model)
        {
            return await _repository.CreateAsync(model);
        }

        public async Task<InstantMeeting?> GetByRoomNameAsync(
            string roomName)
        {
            return await _repository.GetByRoomNameAsync(roomName);
        }
    }
}