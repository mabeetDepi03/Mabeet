using System.ComponentModel.DataAnnotations;

public class UpdateBookingStatusDto
{
    [Required]
    public string Status { get; set; } // "Confirmed", "Cancelled", "Completed"
}
