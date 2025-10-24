using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MabeetEF.Entities
{
    public class LocalLoding:Accommodation
    {
        [Required]
        public double Area { get; set; }
        [Required, Range(1, 100)]
        public int Floor { get; set; }
        [Required, Range(1, 50)]
        public int TotalRooms { get; set; }
        [Range(0, 50)] 
        public int TotalGuests { get; set; }
        [Required, DataType(DataType.Currency), Range(0.0, 15000.0)]
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; } = true;

        // Navigation Properties
        // M:N ==> Booking
        public ICollection<Booking> Bookings { get; set; } = new HashSet<Booking>();   
    }
}
