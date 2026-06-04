using ahello_backend.DbContexts;
using ahello_backend.Models.Availability;  // SlotResult and SlotQuery live here — keep this
using ahello_backend.Models.UserSlots;
using ahello_backend.Services.Interfaces;
using Dapper;

namespace ahello_backend.Services.Classes
{
    public class SlotService : ISlotService
    {
        private readonly DbContext _db;

        public SlotService(DbContext db)
        {
            _db = db;
        }

        public async Task<Dictionary<string, List<SlotResult>>> GetWeekSlots(
     int userId,
     int serviceId,
     int durationMinutes,
     DateTime weekStart)
        {
            using var connection = _db.GetConnection();

            var weekEnd = weekStart.AddDays(6);

            // BUILD which days-of-week and days-of-month fall in this week
            var weekDays = Enumerable.Range(0, 7)
                .Select(i => (int)weekStart.AddDays(i).DayOfWeek)
                .ToList();

            var monthDays = Enumerable.Range(0, 7)
                .Select(i => weekStart.AddDays(i).Day)
                .ToList();

            // GET matching availability windows
            var windowSql = @"
        SELECT
            SlotId,
            StartTime,
            EndTime,
            SlotDate,
            RecurrenceType,
            DayOfWeek,
            DayOfMonth
        FROM userslots
        WHERE UserId    = @UserId
          AND ServiceId = @ServiceId
          AND (
              (RecurrenceType = 'SpecificDate'
                AND SlotDate BETWEEN @WeekStart AND @WeekEnd)
              OR RecurrenceType = 'Daily'
              OR (RecurrenceType = 'Weekly'
                  AND DayOfWeek IN @WeekDays)
              OR (RecurrenceType = 'Monthly'
                  AND DayOfMonth IN @MonthDays)
          )";

            var windows = await connection.QueryAsync<UserSlotGet>(
                windowSql,
                new
                {
                    UserId = userId,
                    ServiceId = serviceId,
                    WeekStart = weekStart.Date,
                    WeekEnd = weekEnd.Date,
                    WeekDays = weekDays,
                    MonthDays = monthDays
                });

            // GET already booked slots for this week
            // GET already booked slots for this week
            var bookedSql = @"
    SELECT StartTime, EndTime, ScheduleDate AS SlotDate
    FROM bookings
    WHERE UserId      = @UserId
      AND ServiceId   = @ServiceId
      AND Status      IN ('Pending', 'Confirmed')
      AND ScheduleDate BETWEEN @WeekStart AND @WeekEnd";

            var booked = await connection.QueryAsync<SlotResult>(
                bookedSql,
                new
                {
                    UserId = userId,
                    ServiceId = serviceId,
                    WeekStart = weekStart.Date,
                    WeekEnd = weekEnd.Date
                });

            // BUILD a hashset of taken slots: "2025-06-02_09:00:00"
            var bookedSet = booked
                .Select(b => $"{b.SlotDate:yyyy-MM-dd}_{b.StartTime}")
                .ToHashSet();

            // GENERATE slots per day
            var result = new Dictionary<string, List<SlotResult>>();

            for (int i = 0; i < 7; i++)
            {
                var date = weekStart.AddDays(i);
                var dateKey = date.ToString("yyyy-MM-dd");
                var slots = new List<SlotResult>();

                var matchingWindows = windows.Where(w =>
                    MatchesDate(w, date));

                foreach (var window in matchingWindows)
                {
                    var current = window.StartTime;

                    while (current + TimeSpan.FromMinutes(durationMinutes)
                           <= window.EndTime)
                    {
                        var slotEnd = current +
                            TimeSpan.FromMinutes(durationMinutes);

                        var key = $"{dateKey}_{current}";

                        if (!bookedSet.Contains(key))
                        {
                            slots.Add(new SlotResult
                            {
                                SlotDate = date,
                                StartTime = current,
                                EndTime = slotEnd,
                                SlotId = window.SlotId
                            });
                        }

                        current = slotEnd;
                    }
                }

                result[dateKey] = slots;
            }

            return result;
        }

        private bool MatchesDate(UserSlotGet w, DateTime date)
        {
            return w.RecurrenceType switch
            {
                "Daily" => true,
                "Weekly" => w.DayOfWeek == (int)date.DayOfWeek,
                "Monthly" => w.DayOfMonth == date.Day,
                "SpecificDate" => w.SlotDate.Date == date.Date,
                _ => false
            };
        }
    }
}