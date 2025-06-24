// NoxEventEazeAppp\Models\NoxEventEazeDBContext.cs
using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Collections.Generic;

namespace NoxEventEazeAppp.Models
{
    public partial class NoxEventEazeDBContext : DbContext
    {
        // **CRITICAL:** Static constructor to disable database initialization.
        // This prevents EF from trying to re-create or alter your database.
        static NoxEventEazeDBContext()
        {
            Database.SetInitializer<NoxEventEazeDBContext>(null);
        }

        public NoxEventEazeDBContext()
            : base("name=NoxEventEazeDBContext")
        {
        }

        // Correct DbSet definitions for all your entities
        public virtual DbSet<Booking> Bookings { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<Venue> Venues { get; set; }
        public virtual DbSet<EventType> EventTypes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // String property configurations for unicode/non-unicode if needed
            modelBuilder.Entity<Event>()
                .Property(e => e.EventName)
                .IsUnicode(false); // If your DB column is VARCHAR

            modelBuilder.Entity<Event>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<Event>()
                .Property(e => e.ImageURL)
                .IsUnicode(false);

            modelBuilder.Entity<Venue>()
                .Property(e => e.VenueName)
                .IsUnicode(false);

            modelBuilder.Entity<Venue>()
                .Property(e => e.Location)
                .IsUnicode(false);

            modelBuilder.Entity<Venue>()
                .Property(e => e.ImageURL)
                .IsUnicode(false);

            modelBuilder.Entity<Booking>()
                .Property(e => e.UserID)
                .IsUnicode(false); // If UserID is VARCHAR in DB

            modelBuilder.Entity<Booking>()
                .Property(e => e.Status)
                .IsUnicode(false); // If Status is VARCHAR in DB

            // **CRITICAL Relationship Mappings:**
            // Ensure these are correctly defined and WillCascadeOnDelete matches your SQL foreign keys.
            // Your SQL script doesn't specify ON DELETE CASCADE, so set to false.

            // One EventType can have many Events (Event.EventTypeID FK)
            modelBuilder.Entity<Event>()
                .HasRequired(e => e.EventType)
                .WithMany(et => et.Events)
                .HasForeignKey(e => e.EventTypeID)
                .WillCascadeOnDelete(false); // Set to false unless you have ON DELETE CASCADE in SQL

            // One Venue can have many Events (Event.VenueID FK)
            modelBuilder.Entity<Event>()
                .HasRequired(e => e.Venue)
                .WithMany(v => v.Events)
                .HasForeignKey(e => e.VenueID)
                .WillCascadeOnDelete(false); // Set to false unless you have ON DELETE CASCADE in SQL

            // One Event can have many Bookings (Booking.EventID FK)
            modelBuilder.Entity<Booking>()
                .HasRequired(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventID)
                .WillCascadeOnDelete(false); // Set to false unless you have ON DELETE CASCADE in SQL

            // This explicitly tells EF that Booking does NOT have a VenueID or Venue property directly.
            // If you somehow have a property like 'public virtual Venue Venue { get; set; }' in Booking.cs,
            // this won't help. Make sure it's removed from Booking.cs!
        }
    }
}