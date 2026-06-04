using ahello_backend.Models.Availability;

namespace ahello_backend.Services.Interfaces
{
    public interface ISlotService
    {
        Task<Dictionary<string, List<SlotResult>>> GetWeekSlots(
      int userId,
      int serviceId,
      int durationMinutes,
      DateTime weekStart);
    }
}