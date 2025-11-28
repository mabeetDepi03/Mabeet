using System.ComponentModel.DataAnnotations;

namespace MabeetApi.DTOs.Property
{
    public class BedCreateDto
    {
        public string RoomDescription { get; set; } // وصف السرير (إن وجد)

        [Required(ErrorMessage = "السعر الليلي مطلوب.")]
        [Range(0.0, 15000.0)]
        public decimal PricePerNight { get; set; }
       public bool IsAvailable { get; set; } = true;
    }
}
