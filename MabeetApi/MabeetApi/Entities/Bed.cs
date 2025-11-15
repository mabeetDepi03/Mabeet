using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MabeetApi.Entities
{
    public class Bed
    {
        [Key]
        public int BedID { get; set; }
        public string RoomDescription { get; set; }
        [Required, DataType(DataType.Currency), Range(0.0, 15000.0)]
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; } = true;

        // Navigation Properties
        // M:N ==> Booking
        public virtual ICollection<Booking> Bookings { get; set; } = new HashSet<Booking>();

        //M:1 ==> StudentRoom
        [Required]
        public int StudentRoomID { get; set; }
        [ForeignKey(nameof(StudentRoomID))]
        public virtual StudentRoom StudentRoom { get; set; }

    }
}
