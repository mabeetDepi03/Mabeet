using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MabeetEF.Entities
{
    public class Booking : IValidatableObject
    {
        [Key]
        public int BookingID { get; set; }

        [Required, DataType(DataType.Currency), Range(0, 100000)]
        public decimal TotalPrice { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime CheckIN { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime CheckOUT { get; set; }

        // Navigation Properties
        // M:1 ==> AppUser
        [Required]
        public string AppUserID { get; set; }
        public virtual AppUser AppUser { get; set; }

        // M:N ==> LocalLoading
        public virtual ICollection<LocalLoding> LocalLodings { get; set; } = new HashSet<LocalLoding>();

        // M:N ==> HotelRoom
        public virtual ICollection<HotelRoom> HotelRooms { get; set; } = new HashSet<HotelRoom>();

        // M:N ==> Bed
        public virtual ICollection<Bed> Beds { get; set; } = new HashSet<Bed>();

        // 1:1 ==> Payment
        public virtual Payment Payment { get; set; }

        // 1:1 ==> Review
        public virtual Review Review { get; set; }


        // Custom Validation: CheckOUT must be after CheckIN
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CheckOUT <= CheckIN)
            {
                yield return new ValidationResult(
                    "CheckOUT must be greater than CheckIN",
                    new[] { nameof(CheckOUT) }
                );
            }
        }
    }
}
