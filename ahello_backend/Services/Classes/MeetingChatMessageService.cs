using ahello_backend.Models.Meeting;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class MeetingChatMessageService
        : IMeetingChatMessageService
    {
        private readonly
            IMeetingChatMessageRepository
            _repository;

        public MeetingChatMessageService(
            IMeetingChatMessageRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MeetingChatMessage>>
            GetAllAsync()
        {
            return await _repository
                .GetAllAsync();
        }

        public async Task<IEnumerable<MeetingChatMessageResponse>>
    GetByMeetingIdAsync(int meetingId)
        {
            return await _repository.GetByMeetingIdAsync(meetingId);
        }

        public async Task<IEnumerable<MeetingChatMessage>>
            GetByUserIdAsync(int userId)
        {
            return await _repository
                .GetByUserIdAsync(userId);
        }

        public async Task<int> CreateAsync(
            MeetingChatMessageCreate model)
        {
            return await _repository
                .CreateAsync(model);
        }

        public async Task<int> UpdateAsync(
            MeetingChatMessageUpdate model)
        {
            return await _repository
                .UpdateAsync(model);
        }

        public async Task<int> DeleteAsync(
            int chatMessageId)
        {
            return await _repository
                .DeleteAsync(chatMessageId);
        }
    }
}