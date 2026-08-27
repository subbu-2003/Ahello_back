using ahello_backend.Models.MeetingParticipant;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class MeetingParticipantService
        : IMeetingParticipantService
    {
        private readonly IMeetingParticipantRepository _repository;
        private readonly IMeetingRecordingService _recordingService;
        private readonly IMeetingRepository _meetingRepository;

        public MeetingParticipantService(
            IMeetingParticipantRepository repository,
            IMeetingRecordingService recordingService,
            IMeetingRepository meetingRepository)
        {
            _repository = repository;
            _recordingService = recordingService;
            _meetingRepository = meetingRepository;
        }


        // =========================================================
        // CREATE PARTICIPANT SESSION
        // =========================================================

        public async Task<int> CreateAsync(
    MeetingParticipant model)
        {
            if (model.MeetingId <= 0)
            {
                throw new ArgumentException(
                    "Invalid MeetingId.");
            }

            if (model.UserId <= 0)
            {
                throw new ArgumentException(
                    "Invalid UserId.");
            }

            // Check whether the user already has
            // an active attendance session.
            var activeParticipant =
                await _repository.GetActiveAsync(
                    model.MeetingId,
                    model.UserId);

            if (activeParticipant != null)
            {
                return activeParticipant.MeetingParticipantId;
            }

            // If JoinedAt is not supplied,
            // use the current server time.
            if (model.JoinedAt == default)
            {
                model.JoinedAt = DateTime.Now;
            }

            // Create participant session
            var participantId =
                await _repository.CreateAsync(model);

            // Check active participants in this meeting
            var activeCount =
                await _repository.GetActiveParticipantCountAsync(
                    model.MeetingId);

            // FIRST PERSON JOINED
            if (activeCount == 1)
            {
                var meeting = await _meetingRepository
                    .GetByIdAsync(model.MeetingId);

                if (meeting == null)
                {
                    throw new ArgumentException("Meeting not found.");
                }

                if (string.IsNullOrWhiteSpace(meeting.RoomId))
                {
                    throw new ArgumentException("RoomId not found.");
                }

                if (string.IsNullOrWhiteSpace(meeting.MeetingLink))
                {
                    throw new ArgumentException("MeetingLink not found.");
                }

                var roomName = meeting.MeetingLink
                    .TrimEnd('/')
                    .Split('/')
                    .Last();

                await _recordingService.StartAsync(
                    new MeetingRecording
                    {
                        MeetingId = meeting.MeetingId,
                        RoomId = meeting.RoomId,
                        RoomName = roomName
                    });
            }

            return participantId;
        }


        // =========================================================
        // LEAVE PARTICIPANT SESSION
        // =========================================================

        public async Task<bool> LeaveAsync(
    MeetingParticipantLeave model)
        {
            if (model.MeetingParticipantId <= 0)
            {
                throw new ArgumentException(
                    "Invalid MeetingParticipantId.");
            }

            // Get participant before closing session
            var participant =
                await _repository.GetByIdAsync(
                    model.MeetingParticipantId);

            if (participant == null || participant.LeftAt != null)
            {
                return false;
            }

            // Close participant session
            var result =
                await _repository.LeaveAsync(
                    model.MeetingParticipantId);

            if (!result)
            {
                return false;
            }

            // Check remaining active participants
            var activeCount =
                await _repository.GetActiveParticipantCountAsync(
                    participant.MeetingId);

            // LAST PERSON LEFT
            if (activeCount == 0)
            {
                await _recordingService.StopAsync(
                    participant.MeetingId);
            }

            return true;
        }


        // =========================================================
        // GET COMPLETE MEETING LOG
        // =========================================================

        public async Task<IEnumerable<MeetingParticipantLog>>
            GetMeetingLogAsync(
                int meetingId)
        {
            if (meetingId <= 0)
            {
                throw new ArgumentException(
                    "Invalid MeetingId.");
            }

            return await _repository
                .GetByMeetingIdAsync(meetingId);
        }


        // =========================================================
        // GET USER PARTICIPATION HISTORY
        // =========================================================

        public async Task<ParticipantMeetingLogResponse?>
            GetParticipantHistoryAsync(
                int meetingId,
                int userId)
        {
            if (meetingId <= 0)
            {
                throw new ArgumentException(
                    "Invalid MeetingId.");
            }

            if (userId <= 0)
            {
                throw new ArgumentException(
                    "Invalid UserId.");
            }

            return await _repository
                .GetByParticipantAsync(
                    meetingId,
                    userId);
        }
        // =========================================================
        // GET ALL MEETING LOGS (GROUPED)
        // =========================================================

        public async Task<IEnumerable<MeetingWithParticipantsResponse>>
            GetAllAsync()
        {
            var rows = await _repository.GetAllAsync();

            var result = rows
                .GroupBy(x => x.MeetingId)
                .Select(g =>
                {
                    var participants = g
                        .Select(x => new ParticipantSessionInfo
                        {
                            MeetingParticipantId = x.MeetingParticipantId,
                            UserId = x.UserId,
                            UserName = x.UserName,
                            ParticipantType = x.ParticipantType,
                            JoinedAt = x.JoinedAt,
                            LeftAt = x.LeftAt,
                            DurationSeconds = x.DurationSeconds,
                            Duration = x.Duration
                        })
                        .OrderBy(x => x.JoinedAt)
                        .ToList();

                    var totalDurationSeconds =
                        participants.Sum(x => x.DurationSeconds ?? 0);

                    return new MeetingWithParticipantsResponse
                    {
                        MeetingId = g.Key,
                        ParticipantCount = participants.Count,
                        TotalDurationSeconds = totalDurationSeconds,
                        TotalDuration = FormatDuration(totalDurationSeconds),
                        Recording = null,
                        Participants = participants
                    };
                })
                .OrderByDescending(x => x.MeetingId)
                .ToList();

            return result;
        }

        // =========================================================
        // GET ALL MEETINGS WITH PARTICIPANTS + RECORDING VIDEO
        // =========================================================

        public async Task<IEnumerable<MeetingWithParticipantsResponse>>
            GetAllWithRecordingAsync()
        {
            var rows = await _repository.GetAllWithRecordingAsync();

            var result = rows
                .GroupBy(x => x.MeetingId)
                .Select(g =>
                {
                    var participants = g
                        .Select(x => new ParticipantSessionInfo
                        {
                            MeetingParticipantId = x.MeetingParticipantId,
                            UserId = x.UserId,
                            UserName = x.UserName,
                            ParticipantType = x.ParticipantType,
                            JoinedAt = x.JoinedAt,
                            LeftAt = x.LeftAt,
                            DurationSeconds = x.DurationSeconds,
                            Duration = x.Duration
                        })
                        .OrderBy(x => x.JoinedAt)
                        .ToList();

                    var totalDurationSeconds =
                        participants.Sum(x => x.DurationSeconds ?? 0);

                    var first = g.First();

                    return new MeetingWithParticipantsResponse
                    {
                        MeetingId = g.Key,
                        ParticipantCount = participants.Count,
                        TotalDurationSeconds = totalDurationSeconds,
                        TotalDuration = FormatDuration(totalDurationSeconds),
                        Recording = first.RecordingId == null
                            ? null
                            : new MeetingRecordingInfo
                            {
                                RecordingId = first.RecordingId,
                                RecordingUrl = first.RecordingUrl,
                                FileName = first.FileName,
                                Status = first.RecordingStatus,
                                StartedAt = first.RecordingStartedAt,
                                EndedAt = first.RecordingEndedAt,
                                DurationSeconds = first.RecordingDurationSeconds
                            },
                        Participants = participants
                    };
                })
                .OrderByDescending(x => x.MeetingId)
                .ToList();

            return result;
        }


        // =========================================================
        // FORMAT DURATION (helper)
        // =========================================================

        private static string FormatDuration(int totalSeconds)
        {
            var hours = totalSeconds / 3600;
            var minutes = (totalSeconds % 3600) / 60;
            var seconds = totalSeconds % 60;

            if (hours > 0) return $"{hours} hr {minutes} min {seconds} sec";
            if (minutes > 0) return $"{minutes} min {seconds} sec";
            return $"{seconds} sec";
        }
    }
}