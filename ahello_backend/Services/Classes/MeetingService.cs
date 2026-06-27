using ahello_backend.Models.Meeting;
using ahello_backend.Models.Pagination;
using ahello_backend.Repositorys.Classes;
using ahello_backend.Repositorys.Interfaces;
using ahello_backend.Services.Interfaces;

namespace ahello_backend.Services.Classes
{
    public class MeetingService : IMeetingService
    {
        private readonly IMeetingRepository _repo;
        private readonly IEscrowPaymentRepository _escrowRepo;
        private readonly IEscrowPaymentLogRepository _logRepo;
        private readonly IRazorpayService _razorpayService;

        public MeetingService(
      IMeetingRepository repo,
      IEscrowPaymentRepository escrowRepo,
      IEscrowPaymentLogRepository logRepo,
      IRazorpayService razorpayService)
        {
            _repo = repo;
            _escrowRepo = escrowRepo;
            _logRepo = logRepo;
            _razorpayService = razorpayService;
        }

        public async Task<IEnumerable<Meeting>> GetAllAsync()
            => await _repo.GetAllAsync();

        public async Task<Meeting> GetByIdAsync(int meetingId)
            => await _repo.GetByIdAsync(meetingId);

        public async Task<IEnumerable<Meeting>> GetByBookingIdAsync(int bookingId)
            => await _repo.GetByBookingIdAsync(bookingId);
        public async Task<Meeting> GetByRoomNameAsync(string roomName)
        {
            return await _repo.GetByRoomNameAsync(roomName);
        }

        public async Task<PagedResult<Meeting>> GetByUserIdAsync(int userId,int pageNumber,int pageSize,string? status,DateTime? startDate)
            => await _repo.GetByUserIdAsync( userId,pageNumber,pageSize, status,startDate);

        public async Task<int> CreateAsync(MeetingPost model)
            => await _repo.CreateAsync(model);

        public async Task<bool> UpdateAsync(MeetingPut model)
        {
            var result = await _repo.UpdateAsync(model);

            if (result && !string.IsNullOrEmpty(model.Status) &&
                model.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                _ = Task.Run(async () =>
                    await OnMeetingCompletedAsync(model.BookingId));
            }

            return result;
        }


        private async Task OnMeetingCompletedAsync(int bookingId)
        {
            var escrow = await _escrowRepo.GetByBookingIdAsync(bookingId);
            if (escrow == null || escrow.Status != "HELD") return;

            try
            {
                var releaseJson = await _razorpayService.ReleaseTransferAsync(
                    escrow.RazorpayTransferId!);

                await _escrowRepo.UpdateReleaseAsync(escrow.EscrowPaymentId, releaseJson);

                await _logRepo.InsertAsync(
                    escrow.EscrowPaymentId,
                    bookingId,
                    "RELEASE_TRANSFER",
                    "SUCCESS",
                    responseJson: releaseJson);
            }
            catch (Exception ex)
            {
                await _logRepo.InsertAsync(
                    escrow.EscrowPaymentId,
                    bookingId,
                    "RELEASE_TRANSFER",
                    "ERROR",
                    errorMessage: ex.Message);
                // Do not rethrow — meeting completion must not fail
            }
        }

        public async Task<bool> DeleteAsync(int meetingId)
            => await _repo.DeleteAsync(meetingId);
        public async Task<bool> SendMeetingReminderAsync()
        {
            var meetings = await _repo.GetPendingRemindersAsync();

            // Use IST now — same timezone as stored StartTime
            var nowIst = DateTime.UtcNow.AddHours(5).AddMinutes(30);

            foreach (var meeting in meetings)
            {
                int minutesLeft = (int)Math.Round((meeting.StartTime - nowIst).TotalMinutes);

                // Mark FIRST — prevents double-send
                await _repo.UpdateReminderSentAsync(meeting.MeetingId);

                await _repo.SendMeetingReminderMailAsync(meeting, minutesLeft);
            }

            return true;
        }

    }
}