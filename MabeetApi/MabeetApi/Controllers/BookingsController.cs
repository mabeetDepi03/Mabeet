using MabeetApi.DTOs;
using MabeetApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace MabeetApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BookingsController : ControllerBase
	{
		private readonly IBookingService _bookingService;

		public BookingsController(IBookingService bookingService)
		{
			_bookingService = bookingService;
		}

		//حجز جديد
		[HttpPost]
		public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto bookingDto)
		{
			try
			{
				var result = await _bookingService.CreateBookingAsync(bookingDto);
				return Ok(result);
			}
			// 🟢 التعديل: لن نكتفي برسالة ex.Message، سنرجع الحالة الداخلية للخطأ 500
			catch (NullReferenceException ex)
			{
				// إذا كان هناك NullReference في الخدمة، فهذا خطأ في الكود الداخلي
				return StatusCode(500, new { message = "Booking failed due to internal data error (Null Reference). Check service logic.", detail = ex.Message });
			}
			catch (Exception ex)
			{
				// لباقي الأخطاء (مثل أخطاء قاعدة البيانات)
				return BadRequest(new { message = "Booking creation failed.", detail = ex.Message });
			}
		}

		//الحصول على جميع الحجوزات
		[HttpGet]
		public async Task<IActionResult> GetAllBookings()
		{
			try
			{
				var bookings = await _bookingService.GetAllBookingsAsync();
				return Ok(bookings);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		//حجوزات يوزر معين
		[HttpGet("user/{userId}")]
		public async Task<IActionResult> GetUserBookings(string userId)
		{
			try
			{
				var bookings = await _bookingService.GetUserBookingsAsync(userId);
				return Ok(bookings);
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		//بيانات حجز معين
		[HttpGet("{id}")]
		public async Task<IActionResult> GetBooking(int id)
		{
			try
			{
				var booking = await _bookingService.GetBookingByIdAsync(id);
				return Ok(booking);
			}
			catch (ArgumentException)
			{
				return NotFound();
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		//الغاء حجز
		[HttpPut("{id}/cancel")]
		public async Task<IActionResult> CancelBooking(int id)
		{
			try
			{
				var result = await _bookingService.CancelBookingAsync(id);
				return Ok(new { message = "Booking cancelled successfully" });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}

		//حذف حجز
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteBooking(int id)
		{
			try
			{
				var result = await _bookingService.DeleteBookingAsync(id);
				return Ok(new { message = "Booking deleted successfully" });
			}
			catch (Exception ex)
			{
				return BadRequest(new { message = ex.Message });
			}
		}
	}
}