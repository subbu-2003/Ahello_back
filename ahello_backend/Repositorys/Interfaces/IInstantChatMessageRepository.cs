using ahello_backend.Models.Meeting;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IInstantChatMessageRepository
    {
        Task<IEnumerable<InstantChatMessage>> GetAllAsync();

        Task<IEnumerable<InstantChatMessage>> GetByRoomIdAsync(string roomId);

        Task<int> CreateAsync(InstantChatMessageCreate model);

        Task<int> UpdateAsync(InstantChatMessageUpdate model);

        Task<int> DeleteAsync(int instantMessageId);

        Task<int> GetUnreadCountAsync(string roomId);

        Task<int> MarkAsReadAsync(string roomId);
    }
}
