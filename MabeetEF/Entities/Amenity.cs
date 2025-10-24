using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MabeetEF.Entities
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
