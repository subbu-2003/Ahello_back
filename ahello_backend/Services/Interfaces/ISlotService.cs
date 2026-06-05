using ahello_backend.Models.Availability;

namespace ahello_backend.Services.Interfaces
{
    public interface ISlotService
    {
        // Existing — kept for backward compatibility
        Task<Dictionary<string, List<SlotResult>>> GetWeekSlots(
            int userId,
            int serviceId,
            int durationMinutes,
            DateTime weekStart);

        // Phase 1 — returns only dates that have at least 1 free slot (fast, for calendar greying)
        Task<List<string>> GetAvailableDates(
            int userId,
            int serviceId,
            int durationMinutes,
            int year,
            int month);

        // Phase 2 — returns all slots for a single clicked date (fast, single day)
        Task<List<SlotResult>> GetDaySlots(
            int userId,
            int serviceId,
            int durationMinutes,
            DateTime date);
    }
}