using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MabeetEF.Entities
{
    public abstract class Accommodation
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
    }
}
