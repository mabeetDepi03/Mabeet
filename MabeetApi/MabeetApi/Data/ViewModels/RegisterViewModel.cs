using System.ComponentModel.DataAnnotations;

namespace MabeetApi.Data.ViewModels
{
	public class RegisterViewModel
	{
		[Required(ErrorMessage = "First Name is required")]
		[StringLength(50)]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "Last Name is required")]
		[StringLength(50)]
		public string LastName { get; set; }

		[Required(ErrorMessage = "Email is required")]
		[EmailAddress(ErrorMessage = "Invalid Email format")]
		public string Email { get; set; }

		[Required(ErrorMessage = "National ID is required")]
		[StringLength(14, MinimumLength = 14, ErrorMessage = "National ID must be 14 digits")]
		public string NationalID { get; set; }

		[Required(ErrorMessage = "Phone Number is required")]
		[Phone(ErrorMessage = "Invalid Phone Number format")]
		public string PhoneNumber { get; set; }

		[Required(ErrorMessage = "Password is required")]
		[StringLength(20, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 20 characters")]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Required(ErrorMessage = "Confirm Password is required")]
		[DataType(DataType.Password)]
		[Compare("Password", ErrorMessage = "Passwords do not match")]
		public string ConfirmPassword { get; set; }

		// 👇 الحقل الجديد المهم جداً
		[Required(ErrorMessage = "User Type is required (Client or Owner)")]
		public string UserType { get; set; }
	}
}
