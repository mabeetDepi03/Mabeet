using MabeetApi.DTOs;
using MabeetApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace MabeetApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public AvailabilityController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // البحث عن عقارات متاحة حسب التواريخ والمدينة والنوع
        [HttpGet("accommodations")]
        public async Task<IActionResult> GetAvailableAccommodations([FromQuery] AvailabilityCheckDto availabilityDto)
        {
            try
            {
                var accommodations = await _bookingService.GetAvailableAccommodationsAsync(availabilityDto);
                return Ok(accommodations);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // الأيام المتاحة والمحجوزة لعقار معين في شهر وسنة محددين
        [HttpGet("accommodation/{accommodationId}/availability")]
        public async Task<IActionResult> GetAccommodationAvailability(
            int accommodationId,
            [FromQuery] int month,
            [FromQuery] int year)
        {
            try
            {
                var availabilityDto = new AccommodationAvailabilityDto
                {
                    AccommodationID = accommodationId,
                    Month = month,
                    Year = year
                };

                var availability = await _bookingService.GetAccommodationAvailabilityAsync(availabilityDto);
                return Ok(availability);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        
        //عرض الايام المتاحة ل عقار معين 
        [HttpGet("accommodation/{accommodationId}/all-available-dates")]
        public async Task<IActionResult> GetAllAvailableDates(int accommodationId)
        {
            try
            {
                var availableDates = await _bookingService.GetAllAvailableDatesAsync(accommodationId);
                return Ok(availableDates);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}