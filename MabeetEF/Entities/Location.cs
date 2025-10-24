using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MabeetEF.Entities
{
    public class Location
    {
        [Key]
        public int LocationID { get; set; }
        [Required, StringLength(100)]
        public string Region { get; set; }
        [StringLength(100)]
        public string Street { get; set; }

        // Navigation Properties
        // 1:M ==> Accommodation
        public virtual ICollection<Accommodation> Accommodations { get; set; } = new HashSet<Accommodation>();

        // M:1 ==> City
        //[Required]
        public int CityID { get; set; }
        //[ForeignKey(nameof(CityID))]
        public virtual City City { get; set; }

    }
}
