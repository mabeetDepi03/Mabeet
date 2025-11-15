using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MabeetApi.Entities
{
    public class City
    {
        [Key]
        public int CityID { get; set; }
        [Required, StringLength(50)]
        public string CityName { get; set; }

        // Navigation Properties
        // 1:M ==> Location
        public virtual ICollection<Location> Locations { get; set; } = new HashSet<Location>();

        // M:1 ==> Governorate
        [Required]
        public int GovernorateID { get; set; }
        [ForeignKey(nameof(GovernorateID))]
        public virtual Governorate Governorate { get; set; }
    }
}
