using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MabeetApi.Entities
{
    public class Image
    {
        [Key]
        public int ImageID { get; set; }
        [Required, StringLength(500)]
        public string ImageUrl { get; set; }
        [StringLength(150)]
        public string AltText { get; set; }
        public bool IsMain { get; set; } = false;
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        // M:1 ==> Accommodation
        [Required]
        public int AccommodationID { get; set; }
        [ForeignKey(nameof(AccommodationID))]
        public virtual Accommodation Accommodation { get; set; }

        // M:N ==> HotelRoom
        public virtual ICollection<HotelRoom> HotelRooms { get; set; } = new HashSet<HotelRoom>();
    }
}
