using MabeetApi.DTOs;
using MabeetApi.Entities;

namespace MabeetApi.Services
{
    public interface IBookingService
    {
        Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto bookingDto);
        Task<List<BookingResponseDto>> GetAllBookingsAsync();
        Task<List<BookingResponseDto>> GetUserBookingsAsync(string userId);
        Task<BookingResponseDto> GetBookingByIdAsync(int bookingId);
        Task<bool> CancelBookingAsync(int bookingId);
        Task<bool> DeleteBookingAsync(int bookingId);
        Task<List<Accommodation>> GetAvailableAccommodationsAsync(AvailabilityCheckDto availabilityDto);
        Task<AvailabilityResponseDto> GetAccommodationAvailabilityAsync(AccommodationAvailabilityDto availabilityDto);
        Task<List<DateTime>> GetAllAvailableDatesAsync(int accommodationId);
		Task<Accommodation> GetPublicAccommodationByIdAsync(int id);
		// admin
		Task<bool> UpdateBookingStatusAsync(int bookingId, UpdateBookingStatusDto dto);
    }
}
