using ahello_backend.Models.Meeting;

namespace ahello_backend.Services.Interfaces
{
    public interface IInstantChatMessageService
    {
        Task<IEnumerable<InstantChatMessage>> GetAllAsync();

        Task<IEnumerable<InstantChatMessage>> GetByRoomIdAsync(string roomId);

        Task<int> CreateAsync(InstantChatMessageCreate model);

        Task<int> UpdateAsync(InstantChatMessageUpdate model);

        Task<int> DeleteAsync(int instantMessageId);

        Task<int> GetUnreadCountAsync(string roomId,string peerId);

        Task<int> MarkAsReadAsync(string roomId, string peerId);
    }
}
