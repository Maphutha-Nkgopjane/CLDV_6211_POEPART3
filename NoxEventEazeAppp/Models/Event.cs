using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NoxEventEazeAppp.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [Required]
        public string EventName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EventDate { get; set; }

        public string EventTime { get; set; }

        public string Description { get; set; }

        public string ImageURL { get; set; }

        // Foreign Key and Navigation Property for Venue
        [Required] // Matches NOT NULL in your SQL
        public int VenueID { get; set; }
        [ForeignKey("VenueID")]
        public virtual Venue Venue { get; set; }

        [Required]
        public int Capacity { get; set; }

        // Foreign Key and Navigation Property for EventType
        [Required] // Matches NOT NULL in your SQL
        public int EventTypeID { get; set; }
        [ForeignKey("EventTypeID")]
        public virtual EventType EventType { get; set; }

        // Collection for related Bookings
        public virtual ICollection<Booking> Bookings { get; set; }
    }
}