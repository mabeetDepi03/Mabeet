using System.ComponentModel.DataAnnotations;

namespace MabeetApi.DTOs.Property
{
    public class LocalLodingCreationDto
    {
        [Required]
        public double Area { get; set; }

        [Required]
        [Range(1, 100)]
        public int Floor { get; set; }

        [Required]
        [Range(1, 50)]
        public int TotalRooms { get; set; }

        [Range(0, 50)]
        public int TotalGuests { get; set; }

        [Required]
        [Range(0.0, 15000.0)]
        public decimal PricePerNight { get; set; }
    }
}
