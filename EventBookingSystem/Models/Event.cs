using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventBookingSystem.Models
{
    public class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventId { get; set; }
        public required int VenueId { get; set; }
        [ForeignKey("VenueId")]
        public required Venue Venue { get; set; }
        [Required]
        public required string EventName { get; set; }
        [DataType(DataType.DateTime)]
        public required DateTime EventDate { get; set; }
        [Required]
        public required string Description { get; set; }
        public int EventTypeId { get; set; }
        [ForeignKey("EventTypeId")]
        public required EventType EventType { get; set; }
        
    }
}
