namespace MabeetApi.DTOs.Property
{
    public class BedDto
    {
        public int BedID { get; set; }
        public string RoomDescription { get; set; }
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; }
        public int StudentRoomID { get; set; }
    }
}
