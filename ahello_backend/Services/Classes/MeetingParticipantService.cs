using ahello_backend.Models.MeetingParticipant;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class MeetingParticipantService
        : IMeetingParticipantService
    {
        private readonly IMeetingParticipantRepository _repository;

        public MeetingParticipantService(
            IMeetingParticipantRepository repository)
        {
            _repository = repository;
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

            return await _repository.CreateAsync(model);
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

            return await _repository.LeaveAsync(
                model.MeetingParticipantId);
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
    }
}