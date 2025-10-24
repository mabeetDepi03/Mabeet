using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MabeetEF.Entities
{
    public class Hotel:Accommodation
    {
        [Required, Range(1,5)]
        public int StarsRate { get; set; }
        
        // Navigation Properties
        // 1:M ==> HotelRoom
        public virtual ICollection<HotelRoom> HotelRooms { get; set; } = new HashSet<HotelRoom>();
    }
}
