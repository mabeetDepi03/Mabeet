using System.ComponentModel.DataAnnotations;

namespace MabeetApi.Entities
{
    public class Governorate
    {
        [Key]
        public int GovernorateID { get; set; }
        [Required, StringLength(50)]
        public string GovernorateName { get; set; }

        // Navigation Properties
        // 1:M ==> City
        public virtual ICollection<City> Cities { get; set; } = new HashSet<City>();
    }
}
