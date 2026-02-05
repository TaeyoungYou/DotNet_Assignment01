namespace Assignment01.Models
{
    public class Attendee
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
    public class Event
    {
        public int Id { get; set;  }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public List<Attendee> Attendees { get; set; }
    }
}
