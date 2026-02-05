using System.Diagnostics;
using Assignment01.Models;
using Microsoft.AspNetCore.Mvc;

namespace Assignment01.Controllers
{
    public class HomeController : Controller
    {
        private static List<Event> datas = new List<Event> { 
            new Event{Id=0, Title="Career Fair", Date=DateTime.Now, Location="Gym", Attendees = new List<Attendee>()},
            new Event{Id=1, Title="Tech Talks", Date=DateTime.Now, Location="Auditorium", Attendees = new List<Attendee>()},
            new Event{Id=2, Title="Hack Night", Date=DateTime.Now, Location="Library", Attendees = new List<Attendee>()} 
        };

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult EventManager()
        {
            return View(datas);
        }
        public IActionResult ManageAttendees(int id)
        {
            var ev = datas.FirstOrDefault(x => x.Id == id);

            if (ev == null) return NotFound();
            return View(ev);
        }
        [HttpPost]
        public IActionResult Signup(string Name, string Email, int id)
        {
            var ev = datas.FirstOrDefault(x => x.Id == id);

            if(ev == null) return NotFound();

            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email))
            {
                return RedirectToAction("ManageAttendees", ev);
            }
            ev.Attendees.Add(new Attendee { Name = Name, Email = Email });

            return RedirectToAction("ManageAttendees", ev);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
