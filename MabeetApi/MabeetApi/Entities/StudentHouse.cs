using System.ComponentModel.DataAnnotations;

namespace MabeetApi.Entities
{
    public class StudentHouse : Accommodation
    {
        [Required]
        public double Area { get; set; }
        [Required, Range(1, 100)]
        public int Floor { get; set; }
        [Range(0, 50)]
        public int TotalGuests { get; set; }

        // Navigation Properties
        // 1:M ==> StudentRoom
        public virtual ICollection<StudentRoom> StudentRooms { get; set; } = new HashSet<StudentRoom>();
    }
}
