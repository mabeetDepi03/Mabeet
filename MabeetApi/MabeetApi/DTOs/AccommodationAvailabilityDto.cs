using System.ComponentModel.DataAnnotations;

namespace MabeetApi.DTOs
{
    public class AccommodationAvailabilityDto
    {
        [Required]
        public int AccommodationID { get; set; }

        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        [Required]
        [Range(2024, 2030)]
        public int Year { get; set; }
    }

    public class AvailabilityResponseDto
    {
        public List<DateTime> AvailableDates { get; set; } = new();
        public List<DateTime> BookedDates { get; set; } = new();
    }
}
