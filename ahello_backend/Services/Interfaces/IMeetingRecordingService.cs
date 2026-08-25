using ahello_backend.Models.MeetingParticipant;

namespace ahello_backend.Services.Interfaces
{
    public interface IMeetingRecordingService
    {
        Task<int> StartAsync(
            MeetingRecording model);

        Task<bool> StopAsync(
            int meetingId);

        Task<MeetingRecording?>
            GetByMeetingIdAsync(
                int meetingId);

        Task<MeetingRecording?> GetByRoomIdAsync(string roomId);

        Task<IEnumerable<MeetingRecording>>
            GetAllAsync();

        Task<bool> ProcessWebhookAsync(
            string hmsRecordingId,
            string? recordingAssetId,
            string? recordingUrl,
            string? fileName,
            DateTime? endedAt,
            int? durationSeconds,
            string status);

        Task<string?> GetPlayUrlAsync(
            int recordingId);
    }
}