using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using NoxEventEazeAppp.Models;

namespace NoxEventEazeAppp.Controllers
{
    public class EventController : Controller
    {
        private readonly NoxEventEazeDBContext db = new NoxEventEazeDBContext();

        public ActionResult Create()
        {
            PopulateCreateEditDropdowns(null);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "EventID,EventName,EventDate,EventTime,Description,VenueID,Capacity,EventTypeID")]
            Event @event,
            HttpPostedFileBase imageFile)
        {
            TimeSpan parseTime;
            DateTime eventStart = DateTime.MinValue;
            DateTime eventEnd = DateTime.MinValue;

            // Validate EventTime input
            if (string.IsNullOrWhiteSpace(@event.EventTime) || !TimeSpan.TryParse(@event.EventTime, out parseTime))
            {
                ModelState.AddModelError("EventTime", "Please enter a valid time (HH:mm).");
                PopulateCreateEditDropdowns(@event);
                return View(@event);
            }

            if (ModelState.IsValid)
            {
                // Combine EventDate and EventTime
                eventStart = @event.EventDate.Date.Add(parseTime);
                eventEnd = eventStart.AddHours(2); // Event duration = 2 hours

                // Check for past date/time
                if (eventStart < DateTime.Now)
                {
                    ModelState.AddModelError("", "Event date and time cannot be in the past.");
                }

                // Check for conflicting events at the same venue
                var conflictingEvent = db.Events
                    .Where(e => e.VenueID == @event.VenueID)
                    .ToList()
                    .Any(e =>
                    {
                        if (string.IsNullOrWhiteSpace(e.EventTime) || !TimeSpan.TryParse(e.EventTime, out TimeSpan existingTime))
                            return false;

                        var existingStart = e.EventDate.Date.Add(existingTime);
                        var existingEnd = existingStart.AddHours(2);

                        return eventStart < existingEnd && eventEnd > existingStart;
                    });

                if (conflictingEvent)
                {
                    ModelState.AddModelError("", "Venue is already booked during this time slot.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    HandleImageUpload(@event, imageFile, isEdit: false);

                    db.Events.Add(@event);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error saving event: {ex.Message}");
                    ModelState.AddModelError("", "Unable to save event. Please try again later.");
                }
            }

            PopulateCreateEditDropdowns(@event);
            return View(@event);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var @event = db.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .FirstOrDefault(e => e.EventID == id);

            if (@event == null)
                return HttpNotFound();

            return View(@event);
        }

        private void PopulateCreateEditDropdowns(Event @event)
        {
            ViewBag.VenueID = new SelectList(db.Venues, "VenueID", "Name", @event?.VenueID);
            ViewBag.EventTypeID = new SelectList(db.EventTypes, "EventTypeID", "TypeName", @event?.EventTypeID);
        }

        private void HandleImageUpload(Event @event, HttpPostedFileBase imageFile, bool isEdit)
        {
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                string fileName = System.IO.Path.GetFileName(imageFile.FileName);
                string path = System.IO.Path.Combine(Server.MapPath("~/Uploads/Events"), fileName);
                imageFile.SaveAs(path);

                @event.ImageURL = "/Uploads/Events/" + fileName;
            }
        }
    }
}
