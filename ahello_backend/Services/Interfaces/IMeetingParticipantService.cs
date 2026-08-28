using ahello_backend.Models.MeetingParticipant;

namespace ahello_backend.Services.Interfaces
{
    public interface IMeetingParticipantService
    {
        Task<int> CreateAsync(
            MeetingParticipant model);

        Task<bool> LeaveAsync(
            MeetingParticipantLeave model);

        Task<IEnumerable<MeetingParticipantLog>>
            GetMeetingLogAsync(
                int meetingId);

        Task<ParticipantMeetingLogResponse?>
            GetParticipantHistoryAsync(
                int meetingId,
                int userId);
        Task<IEnumerable<MeetingWithParticipantsResponse>> GetAllAsync();
        Task<IEnumerable<MeetingWithParticipantsResponse>> GetAllWithRecordingAsync();
    }
}