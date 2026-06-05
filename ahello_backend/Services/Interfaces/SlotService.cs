using ahello_backend.DbContexts;
using ahello_backend.Models.Availability;
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

        // ================================================================
        // EXISTING — week slots (kept as-is)
        // ================================================================
        public async Task<Dictionary<string, List<SlotResult>>> GetWeekSlots(
            int userId,
            int serviceId,
            int durationMinutes,
            DateTime weekStart)
        {
            using var connection = _db.GetConnection();

            var weekEnd = weekStart.AddDays(6);
            var weekDays = Enumerable.Range(0, 7)
                                    .Select(i => (int)weekStart.AddDays(i).DayOfWeek)
                                    .ToList();
            var weekDaysOfMonth = Enumerable.Range(0, 7)
                                    .Select(i => weekStart.AddDays(i).Day)
                                    .ToList();
            var lastDayFallbacks = Enumerable.Range(0, 7)
                                    .Select(i => weekStart.AddDays(i))
                                    .Where(d => d.Day == DateTime.DaysInMonth(d.Year, d.Month))
                                    .SelectMany(d => Enumerable.Range(d.Day, 31 - d.Day + 1))
                                    .ToList();
            var monthDaysToCheck = weekDaysOfMonth.Concat(lastDayFallbacks).Distinct().ToList();

            var windowSql = @"
                SELECT SlotId, StartTime, EndTime, SlotDate,
                       RecurrenceType, DayOfWeek, DayOfMonth
                FROM userslots
                WHERE UserId    = @UserId
                  AND ServiceId = @ServiceId
                  AND (
                      (RecurrenceType = 'SpecificDate' AND SlotDate BETWEEN @WeekStart AND @WeekEnd)
                      OR RecurrenceType = 'Daily'
                      OR (RecurrenceType = 'Weekly'  AND DayOfWeek  IN @WeekDays)
                      OR (RecurrenceType = 'Monthly' AND DayOfMonth IN @MonthDaysToCheck)
                  )";

            var windows = await connection.QueryAsync<UserSlotGet>(windowSql, new
            {
                UserId = userId,
                ServiceId = serviceId,
                WeekStart = weekStart.Date,
                WeekEnd = weekEnd.Date,
                WeekDays = weekDays,
                MonthDaysToCheck = monthDaysToCheck
            });

            var bookedSql = @"
                SELECT StartTime, EndTime, ScheduleDate AS SlotDate
                FROM bookings
                WHERE UserId    = @UserId
                  AND ServiceId = @ServiceId
                  AND Status    IN ('Pending', 'Confirmed')
                  AND ScheduleDate BETWEEN @WeekStart AND @WeekEnd";

            var booked = await connection.QueryAsync<SlotResult>(bookedSql, new
            {
                UserId = userId,
                ServiceId = serviceId,
                WeekStart = weekStart.Date,
                WeekEnd = weekEnd.Date
            });

            var bookedSet = booked
                .Select(b => $"{b.SlotDate:yyyy-MM-dd}_{b.StartTime}")
                .ToHashSet();

            var result = new Dictionary<string, List<SlotResult>>();

            for (int i = 0; i < 7; i++)
            {
                var date = weekStart.AddDays(i);
                var dateKey = date.ToString("yyyy-MM-dd");
                var slots = new List<SlotResult>();

                foreach (var window in windows.Where(w => MatchesDate(w, date)))
                {
                    var current = window.StartTime;
                    while (current + TimeSpan.FromMinutes(durationMinutes) <= window.EndTime)
                    {
                        var slotEnd = current + TimeSpan.FromMinutes(durationMinutes);
                        if (!bookedSet.Contains($"{dateKey}_{current}"))
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

        // ================================================================
        // PHASE 1 — returns only date strings that have >= 1 free slot
        //           called once on month load — very fast, no slot detail
        // ================================================================
        public async Task<List<string>> GetAvailableDates(
            int userId,
            int serviceId,
            int durationMinutes,
            int year,
            int month)
        {
            using var connection = _db.GetConnection();

            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var totalDays = DateTime.DaysInMonth(year, month);

            var weekDays = Enumerable.Range(0, totalDays)
                .Select(i => (int)monthStart.AddDays(i).DayOfWeek)
                .Distinct()
                .ToList();

            var lastDay = DateTime.DaysInMonth(year, month);
            var monthDaysToCheck = Enumerable.Range(1, lastDay)                    // 1..lastDay
                .Concat(Enumerable.Range(lastDay, 31 - lastDay + 1))               // lastDay..31 (overflow)
                .Distinct()
                .ToList();

            var windowSql = @"
                SELECT SlotId, StartTime, EndTime, SlotDate,
                       RecurrenceType, DayOfWeek, DayOfMonth
                FROM userslots
                WHERE UserId    = @UserId
                  AND ServiceId = @ServiceId
                  AND (
                      (RecurrenceType = 'SpecificDate' AND SlotDate BETWEEN @MonthStart AND @MonthEnd)
                      OR RecurrenceType = 'Daily'
                      OR (RecurrenceType = 'Weekly'  AND DayOfWeek  IN @WeekDays)
                      OR (RecurrenceType = 'Monthly' AND DayOfMonth IN @MonthDaysToCheck)
                  )";

            var windows = await connection.QueryAsync<UserSlotGet>(windowSql, new
            {
                UserId = userId,
                ServiceId = serviceId,
                MonthStart = monthStart.Date,
                MonthEnd = monthEnd.Date,
                WeekDays = weekDays,
                MonthDaysToCheck = monthDaysToCheck
            });

            var bookedSql = @"
                SELECT StartTime, ScheduleDate AS SlotDate
                FROM bookings
                WHERE UserId    = @UserId
                  AND ServiceId = @ServiceId
                  AND Status    IN ('Pending', 'Confirmed')
                  AND ScheduleDate BETWEEN @MonthStart AND @MonthEnd";

            var booked = await connection.QueryAsync<SlotResult>(bookedSql, new
            {
                UserId = userId,
                ServiceId = serviceId,
                MonthStart = monthStart.Date,
                MonthEnd = monthEnd.Date
            });

            var bookedSet = booked
                .Select(b => $"{b.SlotDate:yyyy-MM-dd}_{b.StartTime}")
                .ToHashSet();

            var availableDates = new List<string>();

            for (int i = 0; i < totalDays; i++)
            {
                var date = monthStart.AddDays(i);
                var dateKey = date.ToString("yyyy-MM-dd");

                // Only check — stop as soon as we find 1 free slot (no need to build full list)
                var hasSlot = windows
                    .Where(w => MatchesDate(w, date))
                    .Any(window =>
                    {
                        var current = window.StartTime;
                        while (current + TimeSpan.FromMinutes(durationMinutes) <= window.EndTime)
                        {
                            if (!bookedSet.Contains($"{dateKey}_{current}"))
                                return true; // found at least 1 free slot
                            current += TimeSpan.FromMinutes(durationMinutes);
                        }
                        return false;
                    });

                if (hasSlot)
                    availableDates.Add(dateKey);
            }

            return availableDates; // e.g. ["2026-06-30"]
        }

        // ================================================================
        // PHASE 2 — returns all time slots for a single date
        //           called only when user clicks a date on the calendar
        // ================================================================
        public async Task<List<SlotResult>> GetDaySlots(
            int userId,
            int serviceId,
            int durationMinutes,
            DateTime date)
        {
            using var connection = _db.GetConnection();

            var dayOfWeek = (int)date.DayOfWeek;
            var lastDay = DateTime.DaysInMonth(date.Year, date.Month);

            // If today IS the last day of the month, also pull overflow rows (e.g. DayOfMonth=31 in June)
            var monthDaysToCheck = date.Day == lastDay
                ? Enumerable.Range(date.Day, 31 - date.Day + 1).ToList() // lastDay..31
                : new List<int> { date.Day };

            var windowSql = @"
                SELECT SlotId, StartTime, EndTime, SlotDate,
                       RecurrenceType, DayOfWeek, DayOfMonth
                FROM userslots
                WHERE UserId    = @UserId
                  AND ServiceId = @ServiceId
                  AND (
                      (RecurrenceType = 'SpecificDate' AND SlotDate = @Date)
                      OR RecurrenceType = 'Daily'
                      OR (RecurrenceType = 'Weekly'  AND DayOfWeek  = @DayOfWeek)
                      OR (RecurrenceType = 'Monthly' AND DayOfMonth IN @MonthDaysToCheck)
                  )";

            var windows = await connection.QueryAsync<UserSlotGet>(windowSql, new
            {
                UserId = userId,
                ServiceId = serviceId,
                Date = date.Date,
                DayOfWeek = dayOfWeek,
                MonthDaysToCheck = monthDaysToCheck
            });

            var bookedSql = @"
                SELECT StartTime, ScheduleDate AS SlotDate
                FROM bookings
                WHERE UserId    = @UserId
                  AND ServiceId = @ServiceId
                  AND Status    IN ('Pending', 'Confirmed')
                  AND ScheduleDate = @Date";

            var booked = await connection.QueryAsync<SlotResult>(bookedSql, new
            {
                UserId = userId,
                ServiceId = serviceId,
                Date = date.Date
            });

            var bookedSet = booked
                .Select(b => $"{date:yyyy-MM-dd}_{b.StartTime}")
                .ToHashSet();

            var slots = new List<SlotResult>();
            var dateKey = date.ToString("yyyy-MM-dd");

            foreach (var window in windows.Where(w => MatchesDate(w, date)))
            {
                var current = window.StartTime;
                while (current + TimeSpan.FromMinutes(durationMinutes) <= window.EndTime)
                {
                    var slotEnd = current + TimeSpan.FromMinutes(durationMinutes);
                    if (!bookedSet.Contains($"{dateKey}_{current}"))
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

            return slots;
        }

        // ================================================================
        // MatchesDate — shared by all 3 methods above
        // ================================================================
        private bool MatchesDate(UserSlotGet w, DateTime date)
        {
            return w.RecurrenceType switch
            {
                "Daily" => true,
                "Weekly" => w.DayOfWeek == (int)date.DayOfWeek,
                "Monthly" =>
                    w.DayOfMonth > DateTime.DaysInMonth(date.Year, date.Month)
                        ? date.Day == DateTime.DaysInMonth(date.Year, date.Month)
                        : w.DayOfMonth == date.Day,
                "SpecificDate" => w.SlotDate.Date == date.Date,
                _ => false
            };
        }
    }
}