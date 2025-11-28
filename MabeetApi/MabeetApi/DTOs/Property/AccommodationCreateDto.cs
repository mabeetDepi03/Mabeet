using System.ComponentModel.DataAnnotations;

namespace MabeetApi.DTOs.Property
{
    public class AccommodationCreateDto
    {
        [Required]
        [StringLength(50)]
        public string AccommodationName { get; set; }

        [Required]
        [StringLength(500)]
        public string AccommodationDescription { get; set; }

        [Required]
        public string AccommodationType { get; set; } // "Hotel", "LocalLoding", "StudentHouse"

        // Location
        [Required]
        public LocationDto Location { get; set; } = new LocationDto();

        // Type-specific properties
        public int? StarsRate { get; set; } // For Hotel
        public double? Area { get; set; } // For LocalLoding & StudentHouse
        public int? Floor { get; set; }
        public int? TotalRooms { get; set; }
        public int? TotalGuests { get; set; }
        public decimal? PricePerNight { get; set; } // For LocalLoding

        // Amenities
        public List<int> AmenityIds { get; set; } = new List<int>();
        // For Hotel Rooms
        public List<HotelRoomCreateDto> HotelRooms { get; set; } = new List<HotelRoomCreateDto>();

        // For Student House Rooms
        public List<StudentRoomCreateDto> StudentRooms { get; set; } = new List<StudentRoomCreateDto>();

    }
}
