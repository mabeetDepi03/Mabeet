using System.ComponentModel.DataAnnotations;

namespace MabeetApi.DTOs
{
    public enum BookingStatus { Pending, Confirmed, Cancelled, Completed }
    public class UpdateBookingStatusDto
    {
        [Required]
        public string Status { get; set; } // "Confirmed", "Cancelled", "Completed"
    }
}
