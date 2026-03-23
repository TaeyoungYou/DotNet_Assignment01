using Assignment01.Models;

namespace Assignment01.Data
{
    public class Initializer
    {
        public static void Initialize(EventDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Events.Any()) return;

            var events = new Event[]
            {
                new Event
                {
                    Title = "Global AI Summit 2026",
                    Description = "Exploring the future of Generative AI and Large Language Models.",
                    Date = DateTime.Parse("2026-05-15 09:00"),
                    Location = "Ottawa Convention Centre",
                    BannerUrl = "https://via.placeholder.com/800x400.png?text=AI+Summit",
                    Attendees = new List<Attendee>
                    {
                        new Attendee { Name = "Alice Johnson", Email = "alice.j@example.com" },
                        new Attendee { Name = "Bob Smith", Email = "bob.smith@tech.com" }
                    }
                },
                new Event
                {
                    Title = "Full Stack Web Workshop",
                    Description = "Hands-on session building modern web apps with Next.js and EF Core.",
                    Date = DateTime.Parse("2026-06-10 13:00"),
                    Location = "Algonquin College T-Building",
                    BannerUrl = "https://via.placeholder.com/800x400.png?text=Web+Workshop",
                    Attendees = new List<Attendee>
                    {
                        new Attendee { Name = "Charlie Brown", Email = "charlie@webdev.org" },
                        new Attendee { Name = "Diana Prince", Email = "diana.p@amazon.com" }
                    }
                },
                new Event
                {
                    Title = "Cybersecurity Night",
                    Description = "A networking event for security professionals and students.",
                    Date = DateTime.Parse("2026-07-22 18:30"),
                    Location = "Kanata North Tech Hub",
                    BannerUrl = "https://via.placeholder.com/800x400.png?text=Cyber+Night",
                    Attendees = new List<Attendee>
                    {
                        new Attendee { Name = "Edward Norton", Email = "edward@secure.net" },
                        new Attendee { Name = "Fiona Gallagher", Email = "fiona.g@startup.io" }
                    }
                }
            };

            context.Events.AddRange(events);
            context.SaveChanges();
        }
    }
}
