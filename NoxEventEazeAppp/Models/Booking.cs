// NoxEventEazeAppp\Models\Booking.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NoxEventEazeAppp.Models
{
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }

        [Required]
        public int EventID { get; set; }
        [ForeignKey("EventID")]
        public virtual Event Event { get; set; } // Navigation property to Event

        [Required]
        [StringLength(128)] // Matches NVARCHAR(128) in your SQL
        public string UserID { get; set; }

        [Required]
        public int NumberOfTickets { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime BookingDate { get; set; }

        [StringLength(50)] // Matches VARCHAR(50) in your SQL
        public string Status { get; set; }

        // !!! IMPORTANT: ENSURE THE FOLLOWING LINES ARE ABSOLUTELY NOT PRESENT IN YOUR Booking.cs !!!
        // public int VenueID { get; set; }
        // public virtual Venue Venue { get; set; }
    }
}