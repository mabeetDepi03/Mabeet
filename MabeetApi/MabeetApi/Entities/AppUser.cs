using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MabeetApi.Entities
{
    public enum UserRole
    {
        Admin = 1,
        Owner = 2,
        Client = 3
    }
    public class AppUser : IdentityUser
    {
        [Required, StringLength(50)]
        public string FirstName { get; set; }
        [Required, StringLength(50)]
        public string LastName { get; set; }
        [Required]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "National ID must be 14 digits.")]
        public string NationalID { get; set; }
        [MaxLength(255)]
        public string? IDPicture { get; set; }
        [MaxLength(255)]
        public string? ProfilePicture { get; set; }
        [Required]
        public UserRole type { get; set; }
        [DataType(DataType.DateTime)]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        // 1:M ==> Accommodation
        public virtual ICollection<Accommodation> Accommodations { get; set; } = new HashSet<Accommodation>();

        // 1:M ==> Booking
        public virtual ICollection<Booking> Bookings { get; set; } = new HashSet<Booking>();

        // 1:M ==> Favorite
        public virtual ICollection<Favorite> Favorites { get; set; } = new HashSet<Favorite>();
    }
}
