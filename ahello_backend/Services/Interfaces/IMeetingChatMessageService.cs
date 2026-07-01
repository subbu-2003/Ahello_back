using ahello_backend.Models.Meeting;

namespace ahello_backend.Services.Interfaces
{
    public interface IMeetingChatMessageService
    {
        Task<IEnumerable<MeetingChatMessage>>
            GetAllAsync();

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
