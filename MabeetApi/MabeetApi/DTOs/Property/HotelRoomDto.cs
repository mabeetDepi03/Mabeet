using MabeetApi.Entities;

namespace MabeetApi.DTOs.Property
{
    public class HotelRoomDto
    {
        public int HotelRoomID { get; set; }
        public int RoomNumber { get; set; }
        public RoomType Type { get; set; }
        public string RoomDescription { get; set; }
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; }
        public int AccommodationID { get; set; }

        public List<ImageDto> Images { get; set; } = new List<ImageDto>();
    }
}
