namespace MabeetApi.DTOs.Admin.Accommodations
{
    public class AdminAccommodationListDto
    {
        public int AccommodationID { get; set; }
        public string AccommodationName { get; set; }
        public string AccommodationType { get; set; }
        public string OwnerName { get; set; }
        public string CityName { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
