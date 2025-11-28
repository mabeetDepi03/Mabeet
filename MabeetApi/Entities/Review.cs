using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MabeetApi.Entities
{
    public class Review
    {
        [Key]
        public int ReviewID { get; set; }
        [StringLength(200, MinimumLength = 5), Display(Name = "Review")]
        public String Reviewtext { get; set; }
        [Range(1, 5)]
        public int Rate { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        // 1:1 ==> Booking
        [Required]
        public int BookingID { get; set; }
        [ForeignKey(nameof(BookingID))]
        public virtual Booking Booking { get; set; }
    }
}
