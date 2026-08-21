namespace ahello_backend.Services.Interfaces
{
    public interface IWhatsAppService
    {
        Task SendBookingConfirmationWhatsAppAsync(
            string? mobileNumber,
            string clientName,
            string serviceName,
            string date,
            string time,
            int bookingId);

        Task SendMeetingInviteWhatsAppAsync(
            string? mobileNumber,
            string clientName,
            string meetingLink,
            int bookingId);

        Task SendMeetingReminderWhatsAppAsync(
            string? mobileNumber,
            string clientName,
            string otherPersonName,
            string serviceName,
            DateTime startTime,
            string meetingLink,
            int minutesLeft,
            int bookingId);

        Task SendRescheduleConfirmationWhatsAppAsync(
            string? mobileNumber,
            string clientName,
            string serviceName,
            string date,
            string time,
            int bookingId);
    }
}
