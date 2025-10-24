using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MabeetEF.Entities
{
    public class StudentRoom
    {
        [Key]
        public int StudentRoomID { get; set; }
        [Required]
        public int TotalBeds { get; set; }

        // Navigation Properties
        // M:1 ==> StudentHouse
        [Required]
        public int AccommodationID { get; set; }
        [ForeignKey(nameof(AccommodationID))]
        public virtual StudentHouse StudentHouse { get; set; }
        
        // 1:M ==> Bed
        public virtual ICollection<Bed> Beds { get; set; } = new HashSet<Bed>();
    }
}
