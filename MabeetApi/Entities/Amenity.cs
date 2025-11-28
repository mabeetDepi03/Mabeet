using System.ComponentModel.DataAnnotations;

namespace MabeetApi.Entities
{
    public class Amenity
    {
        [Key]
        public int AmenityID { get; set; }
        [Required, StringLength(30)]
        public string AmenityName { get; set; }

        // Navigation Properties
        // M:N ==> Accommodations
        public virtual ICollection<Accommodation> Accommodations { get; set; } = new HashSet<Accommodation>();
    }
}
