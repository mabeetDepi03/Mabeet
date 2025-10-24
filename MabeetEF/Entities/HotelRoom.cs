using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MabeetEF.Entities
{
    public enum RoomType
    {
        Single = 1,
        Double = 2,
    }
    public class HotelRoom
    {
        [Key]
        public int HotelRoomID { get; set; }
        [Required, Range(1, int.MaxValue)]
        public int RoomNumber { get; set; }
        [Required, StringLength(500)]
        public RoomType Type { get; set; }
        public string RoomDescription { get; set; }
        [Required, DataType(DataType.Currency), Range(0.0, 15000.0)]
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; } = true;

        // NAvigation Properties
        // M:1 ==> Hotel 
        [Required]
        public int AccommodationID { get; set; }
        [ForeignKey(nameof(AccommodationID))]
        public virtual Hotel Hotel { get; set; }
        
        // M:N ==> Booking
        public virtual ICollection<Booking> Bookings { get; set; } = new HashSet<Booking>();
        
        // M:N ==> Images
        public virtual ICollection<Image> Images { get; set; } = new HashSet<Image>();
    }
}
