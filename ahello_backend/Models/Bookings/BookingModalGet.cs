namespace ahello_backend.Models.Bookings
{
    public class BookingModalGet
    {
        public int ServiceId { get; set; }

        public string Title { get; set; }

        public string ShortDescription { get; set; }

        public int DurationMinutes { get; set; }

        public decimal Price { get; set; }


        public int UserId { get; set; }

        public string ExpertName { get; set; }

        public string ExpertRole { get; set; }

        public string ExpertImage { get; set; }
    }
}

