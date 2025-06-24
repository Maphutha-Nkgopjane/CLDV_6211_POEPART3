using System;
using System.Collections.Generic; // <--- IMPORTANT: Add this using directive
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace NoxEventEazeAppp.Models
{
    public class EventType
    {
        [Key]
        public int EventTypeID { get; set; } // Changed to non-nullable int for primary key

        [Required]
        [StringLength(100)]
        [Display(Name = "Event Type Name")]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        // --- ADD THIS PROPERTY ---
        // This is the navigation property for the "many" side of the relationship.
        // It tells Entity Framework that one EventType can be associated with many Events.
        public virtual ICollection<Event> Events { get; set; }
        // --- END ADDITION ---
    }
}