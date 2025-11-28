namespace MabeetApi.DTOs
{
    public class BookingResponseDto
    {
        public int BookingID { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CheckIN { get; set; }
        public DateTime CheckOUT { get; set; }
        public string Status { get; set; }
        public string AccommodationName { get; set; }
        public string AccommodationType { get; set; } 
        public DateTime CreatedAt { get; set; }

        // OwnerDetails
        public string OwnerName { get; set; }
        public string OwnerPhone { get; set; }
    }
}
