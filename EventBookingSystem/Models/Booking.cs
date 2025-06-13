using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventBookingSystem.Models
{
    public class Booking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BookingId { get; set; }
        
        public int EventId { get; set; }
       
        public int VenueId { get; set; }
        
        public int CustomerId { get; set; }
        [Required]
        public DateTime BookingDate { get; set; }
        [ForeignKey("EventId")]
        public required Event Event { get; set; }
        [ForeignKey("VenueId")]
        public required Venue Venue { get; set; }
        [ForeignKey("CustomerId")]
        public required Customer Customer { get; set; }
        
    }
}
