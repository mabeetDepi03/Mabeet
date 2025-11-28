using System.ComponentModel.DataAnnotations;

namespace MabeetApi.DTOs.Property
{
    public class StudentRoomUpdateDto
    {
        [Required(ErrorMessage = "Total beds is required")]
        [Range(1, 10, ErrorMessage = "Total beds must be between 1 and 10")]
        public int TotalBeds { get; set; }
    }
}
