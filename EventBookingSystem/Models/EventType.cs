using System.ComponentModel.DataAnnotations;

namespace EventBookingSystem.Models
{
    public class EventType
    {
        [Key]
        public int EventTypeId { get; set; }

        [Required]
        public required string Name { get; set; }
    }
}