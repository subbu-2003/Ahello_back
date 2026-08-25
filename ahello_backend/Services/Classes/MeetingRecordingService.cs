using ahello_backend.Models.MeetingParticipant;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class MeetingRecordingService
        : IMeetingRecordingService
    {
        private readonly IMeetingRecordingRepository _repository;
        private readonly HundredMsService _hundredMsService;

        public MeetingRecordingService(
            IMeetingRecordingRepository repository,
            HundredMsService hundredMsService)
        {
            _repository = repository;
            _hundredMsService = hundredMsService;
        }


        // =========================================================
        // START RECORDING
        // =========================================================

        public async Task<int> StartAsync(
            MeetingRecording model)
        {
            if (model.MeetingId <= 0)
            {
                throw new ArgumentException(
                    "Invalid MeetingId.");
            }

            if (string.IsNullOrWhiteSpace(model.RoomId))
            {
                throw new ArgumentException(
                    "RoomId is required.");
            }


            // -----------------------------------------------------
            // CHECK EXISTING ACTIVE RECORDING
            // -----------------------------------------------------

            var activeRecording =
                await _repository.GetActiveAsync(
                    model.MeetingId);

            if (activeRecording != null)
            {
                return activeRecording.RecordingId;
            }


            // -----------------------------------------------------
            // START RECORDING IN 100MS
            // -----------------------------------------------------

            var hmsResult =
    await _hundredMsService
        .StartRecordingAsync(
            model.RoomId,
            model.RoomName);


            // -----------------------------------------------------
            // SET RECORDING DATA
            // -----------------------------------------------------

            model.HMSRecordingId =
                hmsResult.RecordingId;

            model.StartedAt =
                DateTime.Now;

            model.Status =
                hmsResult.Status;


            // -----------------------------------------------------
            // SAVE RECORDING IN DATABASE
            // -----------------------------------------------------

            return await _repository.CreateAsync(
                model);
        }


        // =========================================================
        // STOP RECORDING
        // =========================================================

        public async Task<bool> StopAsync(
            int meetingId)
        {
            if (meetingId <= 0)
            {
                throw new ArgumentException(
                    "Invalid MeetingId.");
            }


            // -----------------------------------------------------
            // FIND ACTIVE RECORDING
            // -----------------------------------------------------

            var recording =
                await _repository.GetActiveAsync(
                    meetingId);

            if (recording == null)
            {
                return false;
            }


            // -----------------------------------------------------
            // CHECK HMS RECORDING ID
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                    recording.HMSRecordingId))
            {
                throw new InvalidOperationException(
                    "HMSRecordingId not found.");
            }


            // -----------------------------------------------------
            // STOP RECORDING IN 100MS
            // -----------------------------------------------------

            await _hundredMsService
                .StopRecordingAsync(
                    recording.HMSRecordingId);


            // -----------------------------------------------------
            // UPDATE DATABASE
            // -----------------------------------------------------

            return await _repository
                .UpdateStatusAsync(
                    recording.RecordingId,
                    "Stopping");
        }


        // =========================================================
        // GET RECORDING BY MEETING
        // =========================================================

        public async Task<MeetingRecording?>
            GetByMeetingIdAsync(
                int meetingId)
        {
            if (meetingId <= 0)
            {
                throw new ArgumentException(
                    "Invalid MeetingId.");
            }

            return await _repository
                .GetByMeetingIdAsync(
                    meetingId);
        }


        // =========================================================
        // GET ALL RECORDINGS
        // =========================================================

        public async Task<IEnumerable<MeetingRecording>>
            GetAllAsync()
        {
            return await _repository
                .GetAllAsync();
        }


        // =========================================================
        // PROCESS 100MS WEBHOOK
        // =========================================================

        public async Task<bool> ProcessWebhookAsync(
            string hmsRecordingId,
            string? recordingAssetId,
            string? recordingUrl,
            string? fileName,
            DateTime? endedAt,
            int? durationSeconds,
            string status)
        {
            if (string.IsNullOrWhiteSpace(
                    hmsRecordingId))
            {
                throw new ArgumentException(
                    "HMSRecordingId is required.");
            }


            // -----------------------------------------------------
            // FIND RECORDING
            // -----------------------------------------------------

            var recording =
                await _repository
                    .GetByHmsRecordingIdAsync(
                        hmsRecordingId);

            if (recording == null)
            {
                return false;
            }


            // -----------------------------------------------------
            // COMPLETED
            // -----------------------------------------------------

            if (status.Equals(
                    "Completed",
                    StringComparison.OrdinalIgnoreCase))
            {
                return await _repository
                    .UpdateCompletedAsync(
                        recording.RecordingId,
                        recordingAssetId,
                        recordingUrl,
                        fileName,
                        endedAt,
                        durationSeconds);
            }


            // -----------------------------------------------------
            // FAILED
            // -----------------------------------------------------

            if (status.Equals(
                    "Failed",
                    StringComparison.OrdinalIgnoreCase))
            {
                return await _repository
                    .UpdateFailedAsync(
                        recording.RecordingId);
            }


            // -----------------------------------------------------
            // OTHER STATUS
            // -----------------------------------------------------

            return await _repository
                .UpdateStatusAsync(
                    recording.RecordingId,
                    status);
        }

        public async Task<MeetingRecording?> GetByRoomIdAsync(
    string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                throw new ArgumentException(
                    "RoomId is required.");
            }

            return await _repository.GetByRoomIdAsync(roomId);
        }


        // =========================================================
        // GET PLAY URL
        // =========================================================

        public async Task<string?>
            GetPlayUrlAsync(
                int recordingId)
        {
            if (recordingId <= 0)
            {
                throw new ArgumentException(
                    "Invalid RecordingId.");
            }


            // -----------------------------------------------------
            // GET RECORDING FROM DATABASE
            // -----------------------------------------------------

            var recording =
                await _repository.GetByIdAsync(
                    recordingId);

            if (recording == null)
            {
                return null;
            }


            // -----------------------------------------------------
            // RECORDING MUST BE COMPLETED
            // -----------------------------------------------------

            if (!recording.Status.Equals(
                    "Completed",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Recording is not ready for playback.");
            }


            // -----------------------------------------------------
            // ASSET ID REQUIRED
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                    recording.RecordingAssetId))
            {
                throw new InvalidOperationException(
                    "Recording asset is not available.");
            }


            // -----------------------------------------------------
            // GET FRESH URL FROM 100MS
            // -----------------------------------------------------

            var asset =
                await _hundredMsService
                    .GetRecordingAssetAsync(
                        recording.RecordingAssetId);


            // -----------------------------------------------------
            // EXTRACT URL
            // -----------------------------------------------------

            if (!asset.RootElement.TryGetProperty(
                    "recording_presigned_url",
                    out var urlProperty))
            {
                throw new InvalidOperationException(
                    "Recording playback URL was not returned by 100ms.");
            }

            return urlProperty.GetString();
        }
    }
}