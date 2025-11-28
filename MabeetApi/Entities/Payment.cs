using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MabeetApi.Entities
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }
        [Required, StringLength(50), Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }
        [Required, DataType(DataType.Currency), Range(0, 100000)]
        public decimal Amount { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime PayAt { get; set; } = DateTime.Now;

        // Navigation Properties
        // 1:1 ==> Booking
        [Required]
        public int BookingID { get; set; }
        [ForeignKey(nameof(BookingID))]
        public virtual Booking Booking { get; set; }

    }
}
