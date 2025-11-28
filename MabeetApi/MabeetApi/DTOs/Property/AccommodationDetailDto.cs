namespace MabeetApi.DTOs.Property
{
    public class AccommodationDetailDto
    {
        public int AccommodationID { get; set; }
        public string AccommodationName { get; set; }
        public string AccommodationDescription { get; set; }
        public string AccommodationType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Location Information
        public LocationDto Location { get; set; }

        // Host Information
        public string HostId { get; set; }
        public string HostName { get; set; }
        public string HostEmail { get; set; }

        // Type-specific Properties
        public int? StarsRate { get; set; } // Hotel
        public double? Area { get; set; } // LocalLoding & StudentHouse
        public int? Floor { get; set; }
        public int? TotalRooms { get; set; } // LocalLoding
        public int? TotalGuests { get; set; } // LocalLoding
        public decimal? PricePerNight { get; set; } // LocalLoding
        public bool? IsAvailable { get; set; } // LocalLoding

        // Collections
        public List<AmenityDto> Amenities { get; set; } = new List<AmenityDto>();
        public List<ImageDto> Images { get; set; } = new List<ImageDto>();
        public List<HotelRoomDto> HotelRooms { get; set; } = new List<HotelRoomDto>();
        public List<StudentRoomDto> StudentRooms { get; set; } = new List<StudentRoomDto>();
    }
}
