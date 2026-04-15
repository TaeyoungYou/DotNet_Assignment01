using Assignment01.Data;
using Assignment01.Hubs;
using Assignment01.Models;
using Assignment01.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Assignment01.Controllers
{
    [Route("events")]
    public class EventController : Controller
    {
        private readonly EventDbContext _context;
        private readonly BlobService _blobService;
        private readonly IHubContext<EventHub> _hubContext;

        public EventController(EventDbContext context, BlobService blobService, IHubContext<EventHub> hubContext)
        {
            _context = context;
            _blobService = blobService;
            _hubContext = hubContext;
        }

        [AllowAnonymous]
        [Route("")]
        [Route("index")]
        public async Task<IActionResult> Index()
        {
            var events = await _context.Events.ToListAsync();
            return View(events);
        }

        [HttpGet]
        [Authorize(Roles = "Organizer")]
        [Route("create")]
        public IActionResult Create() => View();

        [HttpPost]
        [Authorize(Roles = "Organizer")]
        [Route("create")]
        public async Task<IActionResult> Create(Event @event, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors) Console.WriteLine(error.ErrorMessage);

                return View(@event);
            }

            // Save the identifier name to organizer_user_id
            @event.OrganizerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (imageFile != null && imageFile.Length > 0) 
            {
                @event.BannerUrl = await _blobService.UploadFileAsync(imageFile);
            }

            _context.Add(@event);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@event == null) return NotFound();

            return View(@event);
        }

        [HttpGet]
        [Authorize(Roles = "Organizer")]
        [Route("edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();

            return View(@event);
        }

        [HttpPost]
        [Authorize(Roles = "Organizer")]
        [Route("edit/{id}")]
        public async Task<IActionResult> Edit(int id, Event @event, IFormFile? imageFile)
        {
            if(id != @event.Id) return NotFound();

            if (!ModelState.IsValid) return View(@event);

            if(imageFile != null && imageFile.Length > 0)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                string filePath = Path.Combine(uploadPath, fileName);
                using(var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                //@event.BannerUrl = "/images/" + fileName;
                @event.BannerUrl = await _blobService.UploadFileAsync(imageFile);
            }

            _context.Update(@event);
            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Organizer")]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();
            return View(@event);
        }

        [HttpPost]
        [Authorize(Roles = "Organizer")]
        [Route("Delete/{id}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                if (!string.IsNullOrEmpty(@event.BannerUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", @event.BannerUrl.TrimStart('/'));

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Organizer")]
        [Route("manage-attendees/{id}")]
        public async Task<IActionResult> ManageAttendees(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ev == null) return NotFound();
            return View(ev);
        }

        [HttpGet]
        [Authorize(Roles = "Organizer")]
        [Route("edit-attendees/{id}")]
        public async Task<IActionResult> EditAttendee(int id)
        {
            var attendee = await _context.Attendees.FindAsync(id);
            if (attendee == null) return NotFound();
            return View(attendee);
        }

        [HttpPost]
        [Authorize(Roles = "Organizer")]
        [Route("edit-attendees/{id}")]
        public async Task<IActionResult> EditAttendee(int id, Attendee attendee)
        {
            if (id != attendee.Id) return NotFound();

            _context.Update(attendee);
            await _context.SaveChangesAsync();
            return RedirectToAction("ManageAttendees", new { id = attendee.EventId });
        }

        [HttpPost]
        [Authorize(Roles = "Organizer")]
        [Route("add-attendee")]
        public async Task<IActionResult> AddAttendee(int eventId, string name, string email)
        {
            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(email))
            {
                var attendee = new Attendee { Name = name, Email = email, EventId = eventId };
                _context.Attendees.Add(attendee);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageAttendees), new { id = eventId });
        }

        [HttpPost]
        [Authorize(Roles = "Organizer")]
        [Route("remove-attendee")]
        public async Task<IActionResult> RemoveAttendee(int id, int eventId)
        {
            var attendee = await _context.Attendees.FindAsync(id);
            if (attendee != null)
            {
                _context.Attendees.Remove(attendee);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ManageAttendees), new { id = eventId });
        }

        [HttpPost]
        [Authorize] // Any authenticated user can self-register
        [Route("register/{eventId}")]
        public async Task<IActionResult> Register(int eventId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var userName = User.Identity?.Name;

            var ev = await _context.Events
                .Include(e => e.Attendees)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if(ev == null)
            {
                return NotFound();
            }

            var alreadyRegistered = ev.Attendees.Any(a => a.UserId == userId);

            if (alreadyRegistered)
            {
                return RedirectToAction("Details", new { id = eventId });
            }

            var attendee = new Attendee
            {
                Name = userName,
                Email = userEmail,
                UserId = userId,
                EventId = eventId,
            };

            _context.Attendees.Add(attendee);
            await _context.SaveChangesAsync();

            var attendeeCount = await _context.Attendees.CountAsync(a=>a.EventId == eventId);
            var groupName = $"event-{eventId}";

            await _hubContext.Clients.Group(groupName).SendAsync(
                "AttendeeRegistered",
                attendeeCount,
                attendee.Name,
                attendee.Email
            );

            if (!string.IsNullOrEmpty(ev.OrganizerUserId))
            {
                await _hubContext.Clients.User(ev.OrganizerUserId).SendAsync(
                    "OrganizerNotification",
                    $"{attendee.Email} just registered for your {ev.Title}."
                );
            }

            return RedirectToAction("Details", new { id = eventId });
        }

        [HttpPost]
        [Authorize]
        [Route("unregister/{eventId}")]
        public async Task<IActionResult> Unregister(int eventId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var attendee = await _context.Attendees
                .FirstOrDefaultAsync(a => a.EventId == eventId && a.UserId == userId);

            if(attendee == null)
            {
                return RedirectToAction("Details", new { id = eventId });
            }

            var removedAttendeeName = attendee.Name;
            var removedAttendeeEmail = attendee.Email;

            _context.Attendees.Remove(attendee);
            await _context.SaveChangesAsync();

            var attendeeCount = await _context.Attendees.CountAsync(a => a.EventId == eventId);
            var groupName = $"event-{eventId}";

            await _hubContext.Clients.Group(groupName).SendAsync(
                "AttendeeUnregistered",
                attendeeCount,
                removedAttendeeName,
                removedAttendeeEmail
            );

            return RedirectToAction("Details", new { id = eventId });
        }
    }
}
