using ahello_backend.Models.MeetingParticipant;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IMeetingParticipantRepository
    {
        Task<int> CreateAsync(
            MeetingParticipant model);

        Task<bool> LeaveAsync(
            int meetingParticipantId);

        Task<MeetingParticipant?>
            GetActiveAsync(
                int meetingId,
                int userId);

        Task<IEnumerable<MeetingParticipantLog>>
            GetByMeetingIdAsync(
                int meetingId);

        Task<ParticipantMeetingLogResponse?>
            GetByParticipantAsync(
                int meetingId,
                int userId);
    }
}