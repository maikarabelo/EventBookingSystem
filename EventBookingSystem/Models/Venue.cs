using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EventBookingSystem.Models
{
    public class Venue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VenueId { get; set; }
        [Required]
        public required string VenueName { get; set; }
        [Required]
        public required string Location { get; set; }
        [Required]
        public required int Capacity { get; set; }
        [Required]
        public required string ImageUrl { get; set; }
     
    }
}
