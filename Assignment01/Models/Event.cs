namespace Assignment01.Models
{
    public class Attendee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int EventId { get; set; }
    }
    public class Event
    {
        public int Id { get; set;  }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public string? BannerUrl { get; set; }
        public List<Attendee> Attendees { get; set; } = new List<Attendee>();
    }
}
