using ahello_backend.Models.Meeting;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IMeetingChatMessageRepository
    {
        Task<IEnumerable<MeetingChatMessage>> GetAllAsync();

        Task<IEnumerable<MeetingChatMessageResponse>>
    GetByMeetingIdAsync(int meetingId);

        Task<IEnumerable<MeetingChatMessage>>
            GetByUserIdAsync(int userId);

        Task<int> CreateAsync(
            MeetingChatMessageCreate model);

        Task<int> UpdateAsync(
            MeetingChatMessageUpdate model);

        Task<int> DeleteAsync(int chatMessageId);

    }
}