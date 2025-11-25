namespace MabeetApi.DTOs.Admin.Accommodations
{
    public class AdminAccommodationDetailsDto
    {
        public int AccommodationID { get; set; }
        public string AccommodationName { get; set; }
        public string Description { get; set; }
        public string AccommodationType { get; set; }

        public string OwnerName { get; set; }
        public string OwnerPhone { get; set; }

        public string City { get; set; }
        public string Governorate { get; set; }

        public List<string> Images { get; set; }
        public List<string> Amenities { get; set; }

        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
