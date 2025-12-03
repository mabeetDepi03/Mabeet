using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;
using System.ComponentModel.DataAnnotations.Schema;
namespace MabeetApi.Entities
{
    public class Accommodation
    {
        [Key]
        public int AccommodationID { get; set; }
        [Required, StringLength(50)]
        public string AccommodationName { get; set; }
        [Required, StringLength(500)]
        public string AccommodationDescription { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        public bool IsApproved { get; set; } = false;

        // Navigation Properties
        // M:1 ==> AppUser 
        [Required]
        public string AppUserID { get; set; }
        public virtual AppUser AppUser { get; set; }

        // M:1 ==> Location 
        [Required]
        public int LocationID { get; set; }
        [ForeignKey(nameof(LocationID))]
        public virtual Location Location { get; set; }

        // M:N ==> Favorite
        public virtual ICollection<Favorite> Favorites { get; set; } = new HashSet<Favorite>();

        // M:N ==> Amenities
        public virtual ICollection<Amenity> Amenities { get; set; } = new HashSet<Amenity>();

        // 1:M ==> Images
        public virtual ICollection<Image> Images { get; set; } = new HashSet<Image>();
        public string AccommodationType { get; internal set; }

        public IEnumerable<Booking> Bookings { get; internal set; }
	
		[NotMapped]
		public decimal PricePerNight { get; set; }
	}
}
