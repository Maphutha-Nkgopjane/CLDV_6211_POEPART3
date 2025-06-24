namespace NoxEventEazeAppp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Venue
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Venue()
        {
            Bookings = new HashSet<Booking>();
            Events = new HashSet<Event>();
        }

        public int VenueID { get; set; }

        [Required]
        [StringLength(255)]
        public string VenueName { get; set; }

        [Required]
        [StringLength(255)]
        public string Location { get; set; }

        public int Capacity { get; set; }

        [StringLength(2048)]
        public string ImageURL { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Booking> Bookings { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Event> Events { get; set; }
        public bool IsAvailable { get; internal set; }
    }
}
