namespace MabeetApi.DTOs.Property
{
    public class StudentRoomDto
    {
        public int StudentRoomID { get; set; }
        public int TotalBeds { get; set; }
        public int AccommodationID { get; set; }

        public List<BedDto> Beds { get; set; } = new List<BedDto>();
    }
}
