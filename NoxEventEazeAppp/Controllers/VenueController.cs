using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web; // Required for HttpPostedFileBase and Server.MapPath
using System.Web.Mvc;
using System.IO; // Required for file operations (Path, Directory, File)
using NoxEventEazeAppp.Models;

namespace NoxEventEazeAppp.Controllers
{
    public class VenueController : Controller
    {
        private NoxEventEazeDBContext db = new NoxEventEazeDBContext();

        // GET: Venue
        public ActionResult Index()
        {
            return View(db.Venues.ToList());
        }

        // GET: Venue/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Venue venue = db.Venues.Find(id);
            if (venue == null)
            {
                return HttpNotFound();
            }
            return View(venue);
        }

        // GET: Venue/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Venue/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "VenueID,VenueName,Location,Capacity,ImageURL")] Venue venue,
                                    HttpPostedFileBase imageFile) // Add parameter for image file
        {
            // --- IMAGE UPLOAD LOGIC for Create ---
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(imageFile.FileName)?.ToLower(); // Using ?. for null safety

                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ImageURL", "Invalid file type. Only JPG, JPEG, PNG, GIF are allowed.");
                }
                else if (imageFile.ContentLength > (5 * 1024 * 1024)) // Max 5MB
                {
                    ModelState.AddModelError("ImageURL", "File size exceeds 5MB limit.");
                }
                else
                {
                    try
                    {
                        string fileName = Guid.NewGuid().ToString() + fileExtension;
                        string uploadPath = Server.MapPath("~/Uploads/Images/"); // Using same folder as Events

                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        string path = Path.Combine(uploadPath, fileName);
                        imageFile.SaveAs(path);

                        venue.ImageURL = Url.Content("~/Uploads/Images/" + fileName); // Store relative URL
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("ImageURL", "Error uploading image: " + ex.Message);
                        System.Diagnostics.Debug.WriteLine($"Venue Image upload error (Create): {ex.Message}");
                    }
                }
            }
            else
            {
                // If venue image is required, add validation here:
                // ModelState.AddModelError("ImageURL", "An image is required for the venue.");
            }
            // --- END IMAGE UPLOAD LOGIC ---

            if (ModelState.IsValid)
            {
                db.Venues.Add(venue);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(venue);
        }

        // GET: Venue/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Venue venue = db.Venues.Find(id);
            if (venue == null)
            {
                return HttpNotFound();
            }

            return View(venue);
        }

        // POST: Venue/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "VenueID,VenueName,Location,Capacity,ImageURL")] Venue venue,
                                HttpPostedFileBase imageFile) // Add parameter for image file
        {
            // Optional: Prevent or warn editing if venue has events/bookings
            // Note: This check might be too strict, editing venue details (like ImageURL)
            // while it has future bookings might be acceptable.
            // The current logic simply adds a model error as a warning, not preventing save.
            var hasEvents = db.Events.Any(e => e.VenueID == venue.VenueID);
            if (hasEvents)
            {
                // Consider if this should prevent saving or just be a warning.
                // For now, it's just a warning.
                ModelState.AddModelError("", "This venue has associated events. Changes might affect existing event details.");
            }


            // --- IMAGE UPLOAD LOGIC for Edit ---
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(imageFile.FileName)?.ToLower();

                if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ImageURL", "Invalid file type. Only JPG, JPEG, PNG, GIF are allowed.");
                }
                else if (imageFile.ContentLength > (5 * 1024 * 1024)) // Max 5MB
                {
                    ModelState.AddModelError("ImageURL", "File size exceeds 5MB limit.");
                }
                else
                {
                    try
                    {
                        // Delete old image if it exists and is stored locally
                        if (!string.IsNullOrEmpty(venue.ImageURL) && venue.ImageURL.StartsWith("~/Uploads/Images/"))
                        {
                            string oldImagePath = Server.MapPath(venue.ImageURL);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Save the new image
                        string fileName = Guid.NewGuid().ToString() + fileExtension;
                        string uploadPath = Server.MapPath("~/Uploads/Images/");

                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        string path = Path.Combine(uploadPath, fileName);
                        imageFile.SaveAs(path);

                        venue.ImageURL = Url.Content("~/Uploads/Images/" + fileName); // Update with new relative URL
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("ImageURL", "Error uploading new image: " + ex.Message);
                        System.Diagnostics.Debug.WriteLine($"Venue Image upload error (Edit): {ex.Message}");
                    }
                }
            }
            else
            {
                // If no new file is uploaded, the existing ImageURL from the hidden field in the view
                // will be bound to the 'venue' model. No action needed here to retain the old image.
            }
            // --- END IMAGE UPLOAD LOGIC for Edit ---

            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(venue).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (System.Data.DataException ex) // Catch specific database exceptions
                {
                    System.Diagnostics.Debug.WriteLine($"Error during Venue Edit save: {ex.Message}");
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, contact your system administrator.");
                }
            }
            return View(venue);
        }

        // GET: Venue/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Venue venue = db.Venues.Find(id);
            if (venue == null)
            {
                return HttpNotFound();
            }

            // Check for associated events (which implies bookings)
            var hasAssociatedEvents = db.Events.Any(e => e.VenueID == venue.VenueID);
            if (hasAssociatedEvents)
            {
                ViewBag.DeletionWarning = "This venue has associated events and cannot be deleted until all events linked to it are removed.";
            }

            return View(venue);
        }

        // POST: Venue/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Venue venue = db.Venues.Find(id);

            // Prevent deletion if venue has any associated events (which would then have bookings)
            // It's more robust to check for events directly, as event is the direct foreign key.
            var hasAssociatedEvents = db.Events.Any(e => e.VenueID == id);
            if (hasAssociatedEvents)
            {
                ModelState.AddModelError("", "Cannot delete this venue because it has existing events associated with it. Please delete the events first.");
                // Fetch the venue again with necessary data for the view
                return View(venue);
            }

            if (venue != null)
            {
                // Delete the associated image file from the server
                if (!string.IsNullOrEmpty(venue.ImageURL) && venue.ImageURL.StartsWith("~/Uploads/Images/"))
                {
                    string imagePath = Server.MapPath(venue.ImageURL);
                    if (System.IO.File.Exists(imagePath))
                    {
                        try
                        {
                            System.IO.File.Delete(imagePath);
                        }
                        catch (Exception ex)
                        {
                            // Log the error but don't prevent DB deletion if file deletion fails.
                            System.Diagnostics.Debug.WriteLine($"Error deleting image file for venue ID {id}: {ex.Message}");
                        }
                    }
                }

                db.Venues.Remove(venue);
                db.SaveChanges();
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