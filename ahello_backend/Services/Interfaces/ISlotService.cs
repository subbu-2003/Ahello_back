using ahello_backend.Models.Availability;
using ahello_backend.Models.UserSlots;

namespace ahello_backend.Services.Interfaces
{
    public interface ISlotService
    {
        Task<int> GetServiceDurationAsync(int userId, int serviceId);

        Task<Dictionary<string, List<SlotResult>>> GetWeekSlots(
            int userId,
            int serviceId,
            int durationMinutes,
            DateTime weekStart);

        Task<List<string>> GetAvailableDates(
            int userId,
            int serviceId,
            int durationMinutes,
            int year,
            int month);

        Task<List<SlotResult>> GetDaySlots(
            int userId,
            int serviceId,
            int durationMinutes,
            DateTime date);
    }
}