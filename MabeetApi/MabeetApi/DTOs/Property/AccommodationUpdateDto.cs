using System.ComponentModel.DataAnnotations;

namespace MabeetApi.DTOs.Property
{
    public class AccommodationUpdateDto
    {
        [Required(ErrorMessage = "Accommodation ID is required")]
        public int AccommodationID { get; set; }

        [Required(ErrorMessage = "Accommodation name is required")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public string AccommodationName { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string AccommodationDescription { get; set; }

        public LocationDto? Location { get; set; }
        public List<int> AmenityIds { get; set; } = new List<int>();
        [Range(1, 5)]
        public int? StarsRate { get; set; }
        public double? Area { get; set; }
        public int? Floor { get; set; }
        public int? TotalGuests { get; set; }
        public int? TotalRooms { get; set; }
        public decimal? PricePerNight { get; set; }
        public bool? IsAvailable { get; set; }

    }
}
