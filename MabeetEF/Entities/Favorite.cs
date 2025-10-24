using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MabeetEF.Entities
{
    public class Favorite
    {
        [Key]
        public int FavoriteID { get; set; }
        // Navigation Properties
        // M:1 ==> AppUser
        [Required]
        public string AppUserID { get; set; }
        public virtual AppUser AppUser { get; set; }
        
        // M:N ==> Accommodation
        public virtual ICollection<Accommodation> Accommodations { get; set; } = new HashSet<Accommodation>();
    }
}
