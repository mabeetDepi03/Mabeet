namespace MabeetApi.DTOs.Property
{
    public class ImageDto
    {
        public int ImageID { get; set; }
        public string ImageUrl { get; set; }
        public string AltText { get; set; }
        public bool IsMain { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AccommodationID { get; set; }
    }
}
