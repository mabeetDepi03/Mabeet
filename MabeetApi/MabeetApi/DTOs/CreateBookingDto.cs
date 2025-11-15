using System.ComponentModel.DataAnnotations;

namespace MabeetApi.DTOs
{
    public class CreateBookingDto
    {
        [Required]
        public DateTime CheckIN { get; set; }

        [Required]
        public DateTime CheckOUT { get; set; }

        public int? LocalLodingID { get; set; }
        public int? HotelRoomID { get; set; }
        public int? BedID { get; set; }

        //No JWT
        [Required]
        public string UserId { get; set; }
    }
}
