namespace ahello_backend.Models.Availability
{
    public class SlotResult
    {
        public int SlotId { get; set; }         // ADD
        public DateTime SlotDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Display => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
        public bool IsAvailable { get; set; } = true;
    }

    public class SlotQuery
    {
        public int UserId { get; set; }
        public int ServiceId { get; set; }
        public DateOnly Date { get; set; }
    }

    public class TimeRange
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }

        public TimeRange(TimeSpan start, TimeSpan end)
        {
            Start = start;
            End = end;
        }
    }
}