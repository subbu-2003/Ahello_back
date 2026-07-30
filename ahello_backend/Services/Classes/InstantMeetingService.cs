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
        public async Task<int> CreateJoinRequestAsync(
            InstantMeetingJoinRequest model)
        {
            return await _repository.CreateJoinRequestAsync(model);
        }

        public async Task<IEnumerable<InstantMeetingJoinRequest>> GetWaitingUsersAsync(
            int instantMeetingId)
        {
            return await _repository.GetWaitingUsersAsync(instantMeetingId);
        }

        public async Task<InstantMeetingJoinRequest?> GetJoinRequestByIdAsync(
            int requestId)
        {
            return await _repository.GetJoinRequestByIdAsync(requestId);
        }

        public async Task<int> UpdateJoinRequestStatusAsync(
            int requestId,
            string status)
        {
            return await _repository.UpdateJoinRequestStatusAsync(requestId, status);
        }

        public async Task<int> UpdateAllJoinRequestStatusAsync(
            int instantMeetingId,
            string status)
        {
            return await _repository.UpdateAllJoinRequestStatusAsync(
                instantMeetingId,
                status);
        }
        public async Task<InstantMeetingJoinRequest?> GetByMeetingAndUserAsync(
    int instantMeetingId,
    int userId)
        {
            return await _repository.GetByMeetingAndUserAsync(
                instantMeetingId,
                userId);
        }
    }
}