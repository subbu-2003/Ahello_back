namespace ahello_backend.Models.Whatsapp
{
    public class WhatsAppSettings
    {
        public string Token { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;

        // Template names must already be approved/registered with your
        // WhatsApp Business provider (same as "pluso_invoice_file" in the reference project)
        public string TemplateBookingConfirmation { get; set; } = "ahllo_booking_confirmation";
        public string TemplateMeetingInvite { get; set; } = "ahllo_meeting_invite";
        public string TemplateMeetingReminder { get; set; } = "ahllo_meeting_reminder";
        public string TemplateReschedule { get; set; } = "ahllo_reschedule_confirmation";
    }
}
