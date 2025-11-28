using System.ComponentModel.DataAnnotations;

namespace MabeetApi.Entities
{
    public class Hotel : Accommodation
    {
        [Required, Range(1, 5)]
        public int StarsRate { get; set; }

        // Navigation Properties
        // 1:M ==> HotelRoom
        public virtual ICollection<HotelRoom> HotelRooms { get; set; } = new HashSet<HotelRoom>();
    }
}
