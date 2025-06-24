using System;
using System.Data.Entity; // Needed for .Include()
using System.Diagnostics;
using System.Linq; // Needed for .ToList(), .OrderBy(), .Sum(), .Any()
using System.Net; // Needed for HttpStatusCodeResult
using System.Web.Mvc; // Needed for Controller, ActionResult, SelectList, Bind, ValidateAntiForgeryToken
using NoxEventEazeAppp.Models; // Your application's models

namespace NoxEventEazeAppp.Controllers
{
    public class BookingController : Controller
    {
        private NoxEventEazeDBContext db = new NoxEventEazeDBContext();

        // GET: Bookings
        public ActionResult Index()
        {
            // Correct eager loading: Booking -> Event -> Venue
            // This pulls Event details and then the Venue details associated with that Event.
            // Ensure your Booking model DOES NOT have a VenueID directly.
            IQueryable<Booking> Bookings = db.Bookings.Include(b => b.Event.Venue).Include(b => b.Event);

            // The error "Invalid column name 'VenueID'" on line 31 occurs because
            // EF is trying to map a non-existent VenueID column on Bookings.
            // This is primarily due to a mismatch in your Booking.cs model.
            return View(Bookings.ToList()); // Use .ToList() if action is not async
        }

        // GET: Bookings/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // Include related Event and Venue data for the details view
            Booking booking = db.Bookings.Include(b => b.Event.Venue).Include(b => b.Event).FirstOrDefault(b => b.BookingID == id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // GET: Bookings/Create
        public ActionResult Create()
        {
            // Populate dropdown for events, ordered by EventName for better UX.
            // This ensures the SelectList has items, preventing ArgumentNullException.
            ViewBag.EventID = new SelectList(db.Events.OrderBy(e => e.EventName), "EventID", "EventName");
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BookingID,EventID,UserID,NumberOfTickets,BookingDate,Status")] Booking booking)
        {
            // Set BookingDate to current date/time if not set by form or is default
            if (booking.BookingDate == DateTime.MinValue)
            {
                booking.BookingDate = DateTime.Now;
            }

            // Set UserID: Replace with actual ASP.NET Identity user ID if implemented
            if (string.IsNullOrEmpty(booking.UserID))
            {
                // For demonstration, use a dummy ID. In a real app, use User.Identity.GetUserId()
                booking.UserID = "TemporaryUserID_Default";
            }

            // Set Status: Provide a default status if not set by the form
            if (string.IsNullOrEmpty(booking.Status))
            {
                booking.Status = "Pending";
            }

            // Perform capacity check before saving
            if (ModelState.IsValid)
            {
                var selectedEvent = db.Events.Find(booking.EventID);
                if (selectedEvent != null)
                {
                    // Calculate total booked tickets for this event
                    var totalBookedTickets = db.Bookings
                        .Where(b => b.EventID == booking.EventID)
                        .Sum(b => (int?)b.NumberOfTickets) ?? 0; // Sum of nullable int for safety

                    if (totalBookedTickets + booking.NumberOfTickets > selectedEvent.Capacity)
                    {
                        ModelState.AddModelError("", "Not enough capacity for this event. Please select fewer tickets or another event.");
                    }
                }
                else
                {
                    ModelState.AddModelError("EventID", "Selected event not found.");
                }
            }


            if (ModelState.IsValid)
            {
                try
                {
                    db.Bookings.Add(booking);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (System.Data.DataException ex)
                {
                    Debug.WriteLine($"Error during Booking Create save: {ex.Message}");
                    ModelState.AddModelError("", "Unable to create booking. Try again later.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Generic error during Booking Create save: {ex.Message}");
                    ModelState.AddModelError("", "An unexpected error occurred during booking creation.");
                }
            }

            // Re-populate EventID dropdown if ModelState is invalid and view is returned
            ViewBag.EventID = new SelectList(db.Events.OrderBy(e => e.EventName), "EventID", "EventName", booking.EventID);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Booking booking = db.Bookings.Find(id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            ViewBag.EventID = new SelectList(db.Events.OrderBy(e => e.EventName), "EventID", "EventName", booking.EventID);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BookingID,EventID,UserID,NumberOfTickets,BookingDate,Status")] Booking booking)
        {
            // Ensure BookingDate is not DateTime.MinValue
            if (booking.BookingDate == DateTime.MinValue)
            {
                booking.BookingDate = DateTime.Now; // Default if not provided
            }

            // Retain UserID if it's not being updated via the form (e.g., set by Identity)
            if (string.IsNullOrEmpty(booking.UserID))
            {
                var originalBooking = db.Bookings.AsNoTracking().FirstOrDefault(b => b.BookingID == booking.BookingID);
                if (originalBooking != null)
                {
                    booking.UserID = originalBooking.UserID;
                }
                else
                {
                    booking.UserID = "TemporaryUserID_Default"; // Fallback
                }
            }
            // Set Status (e.g., if it can be changed or needs a default)
            if (string.IsNullOrEmpty(booking.Status))
            {
                booking.Status = "Confirmed"; // Default if not provided
            }


            if (ModelState.IsValid)
            {
                // Capacity check for edit as well
                var selectedEvent = db.Events.Find(booking.EventID);
                if (selectedEvent != null)
                {
                    // Exclude current booking's original tickets from total booked count when editing
                    var totalBookedTickets = db.Bookings
                        .Where(b => b.EventID == booking.EventID && b.BookingID != booking.BookingID)
                        .Sum(b => (int?)b.NumberOfTickets) ?? 0;

                    if (totalBookedTickets + booking.NumberOfTickets > selectedEvent.Capacity)
                    {
                        ModelState.AddModelError("", "Not enough capacity for this event. Please select fewer tickets or another event.");
                    }
                }
                else
                {
                    ModelState.AddModelError("EventID", "Selected event not found.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(booking).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (System.Data.DataException ex)
                {
                    Debug.WriteLine($"Error during Booking Edit save: {ex.Message}");
                    ModelState.AddModelError("", "Unable to save changes. Try again later.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Generic error during Booking Edit save: {ex.Message}");
                    ModelState.AddModelError("", "An unexpected error occurred during booking edit.");
                }
            }
            ViewBag.EventID = new SelectList(db.Events.OrderBy(e => e.EventName), "EventID", "EventName", booking.EventID);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            // Include related Event and Venue for display on delete confirmation
            Booking booking = db.Bookings.Include(b => b.Event.Venue).Include(b => b.Event).FirstOrDefault(b => b.BookingID == id);
            if (booking == null)
            {
                return HttpNotFound();
            }
            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Booking booking = db.Bookings.Find(id);
            if (booking != null)
            {
                try
                {
                    db.Bookings.Remove(booking);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error deleting booking: {ex.Message}");
                    ModelState.AddModelError("", "Unable to delete booking. It might have related data.");
                    return View("Delete", db.Bookings.Include(b => b.Event.Venue).Include(b => b.Event).FirstOrDefault(b => b.BookingID == id));
                }
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}