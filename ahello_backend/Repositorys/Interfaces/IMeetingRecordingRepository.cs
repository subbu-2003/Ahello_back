using ahello_backend.Models.MeetingParticipant;

namespace ahello_backend.Repositorys.Interfaces
{
    public interface IMeetingRecordingRepository
    {
        // =========================================================
        // CREATE RECORDING
        // =========================================================

        Task<int> CreateAsync(
            MeetingRecording model);


        // =========================================================
        // GET ACTIVE RECORDING
        // =========================================================

        Task<MeetingRecording?>
            GetActiveAsync(
                int meetingId);


        // =========================================================
        // GET RECORDING BY ID
        // =========================================================

        Task<MeetingRecording?>
            GetByIdAsync(
                int recordingId);

        Task<MeetingRecording?> GetByRoomIdAsync(string roomId);


        // =========================================================
        // GET LATEST RECORDING FOR MEETING
        // =========================================================

        Task<MeetingRecording?>
            GetByMeetingIdAsync(
                int meetingId);


        // =========================================================
        // GET ALL RECORDINGS
        // =========================================================

        Task<IEnumerable<MeetingRecording>>
            GetAllAsync();


        // =========================================================
        // UPDATE STATUS
        // =========================================================

        Task<bool> UpdateStatusAsync(
            int recordingId,
            string status);


        // =========================================================
        // UPDATE STOPPED RECORDING
        // =========================================================

        Task<bool> UpdateStoppedAsync(
            int recordingId,
            DateTime endedAt,
            int durationSeconds);


        // =========================================================
        // UPDATE COMPLETED RECORDING
        // =========================================================

        Task<bool> UpdateCompletedAsync(
            int recordingId,
            string? recordingAssetId,
            string? recordingUrl,
            string? fileName,
            DateTime? endedAt,
            int? durationSeconds);


        // =========================================================
        // UPDATE FAILED RECORDING
        // =========================================================

        Task<bool> UpdateFailedAsync(
            int recordingId);

        Task<MeetingRecording?>
    GetByHmsRecordingIdAsync(
        string hmsRecordingId);
    }
}