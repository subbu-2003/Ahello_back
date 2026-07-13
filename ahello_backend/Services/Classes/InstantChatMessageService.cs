using ahello_backend.Models.Meeting;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class InstantChatMessageService : IInstantChatMessageService
    {
        private readonly
            IInstantChatMessageRepository _repository;

        public InstantChatMessageService(IInstantChatMessageRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<InstantChatMessage>>
            GetAllAsync()
            => await _repository.GetAllAsync();

        public async Task<IEnumerable<InstantChatMessage>>
            GetByRoomIdAsync(string roomId)
            => await _repository.GetByRoomIdAsync(roomId);

        public async Task<int> CreateAsync(
            InstantChatMessageCreate model)
            => await _repository.CreateAsync(model);

        public async Task<int> UpdateAsync(
            InstantChatMessageUpdate model)
            => await _repository.UpdateAsync(model);

        public async Task<int> DeleteAsync(
            int instantMessageId)
            => await _repository.DeleteAsync(instantMessageId);

        public async Task<int> GetUnreadCountAsync(
            string roomId, string peerId)
            => await _repository.GetUnreadCountAsync(roomId,peerId);

        public async Task<int> MarkAsReadAsync(
            string roomId, string peerId)
            => await _repository.MarkAsReadAsync(roomId, peerId);
    }
}