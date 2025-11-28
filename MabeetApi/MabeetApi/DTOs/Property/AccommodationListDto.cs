namespace MabeetApi.DTOs.Property
{
    public class AccommodationListDto
    {
        public int AccommodationID { get; set; }
        public string AccommodationName { get; set; }
        public string AccommodationType { get; set; }
        public DateTime CreatedAt { get; set; }

        // Location Summary
        public string Region { get; set; }
        public string CityName { get; set; }
        public string GovernorateName { get; set; }

        // Main Image
        public string MainImageUrl { get; set; }
        public string MainImageAltText { get; set; }

        // Basic Info
        public int? StarsRate { get; set; } // For Hotel
        public decimal? PricePerNight { get; set; } // For LocalLoding
        public bool? IsAvailable { get; set; } // For LocalLoding

       
    }
}
