using MabeetApi.Data;
using MabeetApi.DTOs;
using MabeetApi.Entities;
using Microsoft.EntityFrameworkCore;
using System.Transactions;

namespace MabeetApi.Services
{
    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;

        public BookingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(CreateBookingDto bookingDto)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            // Validate dates
            if (bookingDto.CheckOUT <= bookingDto.CheckIN)
                throw new ArgumentException("CheckOUT must be after CheckIN");

            if (bookingDto.CheckIN <= DateTime.Now.Date)
                throw new ArgumentException("CheckIN must be in the future");

            // Validate user exists
            var user = await _context.Users.FindAsync(bookingDto.UserId);
            if (user == null)
                throw new ArgumentException("User not found");

            // Determine accommodation type and validate availability
            var (accommodation, totalPrice) = await ValidateAndGetAccommodationAsync(bookingDto);

            // Create booking
            var booking = new Booking
            {
                CheckIN = bookingDto.CheckIN,
                CheckOUT = bookingDto.CheckOUT,
                TotalPrice = totalPrice,
                AppUserID = bookingDto.UserId,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            // Add to appropriate collection based on type
            if (bookingDto.LocalLodingID.HasValue)
            {
                var localLoding = await _context.LocalLodings.FindAsync(bookingDto.LocalLodingID.Value);
                if (localLoding != null)
                    booking.LocalLodings.Add(localLoding);
            }
            else if (bookingDto.HotelRoomID.HasValue)
            {
                var hotelRoom = await _context.HotelRooms.FindAsync(bookingDto.HotelRoomID.Value);
                if (hotelRoom != null)
                    booking.HotelRooms.Add(hotelRoom);
            }
            else if (bookingDto.BedID.HasValue)
            {
                var bed = await _context.Beds.FindAsync(bookingDto.BedID.Value);
                if (bed != null)
                    booking.Beds.Add(bed);
            }

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            scope.Complete();

            return MapToBookingResponseDto(booking);
        }

        public async Task<List<BookingResponseDto>> GetAllBookingsAsync()
        {
            var bookings = await _context.Bookings
                .Include(b => b.LocalLodings)
                .Include(b => b.HotelRooms)
                    .ThenInclude(hr => hr.Hotel)
                .Include(b => b.Beds)
                    .ThenInclude(bed => bed.StudentRoom)
                        .ThenInclude(sr => sr.StudentHouse)
                .Include(b => b.AppUser)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bookings.Select(MapToBookingResponseDto).ToList();
        }

        public async Task<List<BookingResponseDto>> GetUserBookingsAsync(string userId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.LocalLodings)
                .Include(b => b.HotelRooms)
                    .ThenInclude(hr => hr.Hotel)
                .Include(b => b.Beds)
                    .ThenInclude(bed => bed.StudentRoom)
                        .ThenInclude(sr => sr.StudentHouse)
                .Where(b => b.AppUserID == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return bookings.Select(MapToBookingResponseDto).ToList();
        }

        public async Task<BookingResponseDto> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.LocalLodings)
                .Include(b => b.HotelRooms)
                    .ThenInclude(hr => hr.Hotel)
                .Include(b => b.Beds)
                    .ThenInclude(bed => bed.StudentRoom)
                        .ThenInclude(sr => sr.StudentHouse)
                .Include(b => b.AppUser)
                .FirstOrDefaultAsync(b => b.BookingID == bookingId);

            if (booking == null)
                throw new ArgumentException("Booking not found");

            return MapToBookingResponseDto(booking);
        }

        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingID == bookingId);

            if (booking == null)
                throw new ArgumentException("Booking not found");

            // Check if booking can be cancelled
            if (booking.Status == "Cancelled")
                throw new InvalidOperationException("Booking is already cancelled");

            if (booking.Status == "Completed")
                throw new InvalidOperationException("Cannot cancel completed booking");

            booking.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
                throw new ArgumentException("Booking not found");

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return true;
        }

        // باقي الـ Methods كما هي بدون أي authentication
        private async Task<(object accommodation, decimal totalPrice)> ValidateAndGetAccommodationAsync(CreateBookingDto bookingDto)
        {
            var days = (bookingDto.CheckOUT - bookingDto.CheckIN).Days;

            if (bookingDto.LocalLodingID.HasValue)
            {
                var localLoding = await _context.LocalLodings
                    .Include(l => l.Bookings)
                    .FirstOrDefaultAsync(l => l.AccommodationID == bookingDto.LocalLodingID.Value);

                if (localLoding == null) throw new ArgumentException("LocalLoding not found");
                if (!localLoding.IsAvailable) throw new InvalidOperationException("LocalLoding is not available");
                if (!await IsAvailableAsync(localLoding.Bookings, bookingDto.CheckIN, bookingDto.CheckOUT))
                    throw new InvalidOperationException("LocalLoding is not available for the selected dates");

                return (localLoding, localLoding.PricePerNight * days);
            }
            else if (bookingDto.HotelRoomID.HasValue)
            {
                var hotelRoom = await _context.HotelRooms
                    .Include(h => h.Bookings)
                    .FirstOrDefaultAsync(h => h.HotelRoomID == bookingDto.HotelRoomID.Value);

                if (hotelRoom == null) throw new ArgumentException("HotelRoom not found");
                if (!hotelRoom.IsAvailable) throw new InvalidOperationException("HotelRoom is not available");
                if (!await IsAvailableAsync(hotelRoom.Bookings, bookingDto.CheckIN, bookingDto.CheckOUT))
                    throw new InvalidOperationException("HotelRoom is not available for the selected dates");

                return (hotelRoom, hotelRoom.PricePerNight * days);
            }
            else if (bookingDto.BedID.HasValue)
            {
                var bed = await _context.Beds
                    .Include(b => b.Bookings)
                    .FirstOrDefaultAsync(b => b.BedID == bookingDto.BedID.Value);

                if (bed == null) throw new ArgumentException("Bed not found");
                if (!bed.IsAvailable) throw new InvalidOperationException("Bed is not available");
                if (!await IsAvailableAsync(bed.Bookings, bookingDto.CheckIN, bookingDto.CheckOUT))
                    throw new InvalidOperationException("Bed is not available for the selected dates");

                return (bed, bed.PricePerNight * days);
            }

            throw new ArgumentException("Must provide either LocalLodingID, HotelRoomID, or BedID");
        }

        private async Task<bool> IsAvailableAsync(ICollection<Booking> existingBookings, DateTime checkIN, DateTime checkOUT)
        {
            return !await Task.Run(() => existingBookings.Any(b =>
                b.Status != "Cancelled" &&
                ((checkIN >= b.CheckIN && checkIN < b.CheckOUT) ||
                 (checkOUT > b.CheckIN && checkOUT <= b.CheckOUT) ||
                 (checkIN <= b.CheckIN && checkOUT >= b.CheckOUT))));
        }

        public async Task<List<Accommodation>> GetAvailableAccommodationsAsync(AvailabilityCheckDto availabilityDto)
        {
            var query = _context.Accommodations
                .Include(a => a.Location)
                    .ThenInclude(l => l.City)
                .Include(a => a.Images)
                .AsQueryable();

            // Filter by city
            if (availabilityDto.CityID.HasValue)
            {
                query = query.Where(a => a.Location.CityID == availabilityDto.CityID.Value);
            }

            // Filter by accommodation type
            if (!string.IsNullOrEmpty(availabilityDto.AccommodationType))
            {
                query = query.Where(a => a.AccommodationType == availabilityDto.AccommodationType);
            }

            var accommodations = await query.ToListAsync();

            // Filter by availability for dates
            var availableAccommodations = new List<Accommodation>();

            foreach (var accommodation in accommodations)
            {
                var isAvailable = await IsAccommodationAvailableAsync(accommodation, availabilityDto.CheckIN, availabilityDto.CheckOUT);
                if (isAvailable)
                {
                    availableAccommodations.Add(accommodation);
                }
            }

            return availableAccommodations;
        }

        private async Task<bool> IsAccommodationAvailableAsync(Accommodation accommodation, DateTime checkIN, DateTime checkOUT)
        {
            switch (accommodation)
            {
                case Hotel hotel:
                    var availableRooms = await _context.HotelRooms
                        .Where(hr => hr.AccommodationID == hotel.AccommodationID && hr.IsAvailable)
                        .ToListAsync();

                    foreach (var room in availableRooms)
                    {
                        var isRoomAvailable = await IsAvailableAsync(room.Bookings, checkIN, checkOUT);
                        if (isRoomAvailable) return true;
                    }
                    return false;

                case LocalLoding localLoding:
                    if (!localLoding.IsAvailable) return false;
                    return await IsAvailableAsync(localLoding.Bookings, checkIN, checkOUT);

                case StudentHouse studentHouse:
                    var availableBeds = await _context.Beds
                        .Where(b => b.StudentRoom.StudentHouse.AccommodationID == studentHouse.AccommodationID && b.IsAvailable)
                        .ToListAsync();

                    foreach (var bed in availableBeds)
                    {
                        var isBedAvailable = await IsAvailableAsync(bed.Bookings, checkIN, checkOUT);
                        if (isBedAvailable) return true;
                    }
                    return false;

                default:
                    return false;
            }
        }

        private BookingResponseDto MapToBookingResponseDto(Booking booking)
        {
            var dto = new BookingResponseDto
            {
                BookingID = booking.BookingID,
                TotalPrice = booking.TotalPrice,
                CheckIN = booking.CheckIN,
                CheckOUT = booking.CheckOUT,
                Status = booking.Status,
                CreatedAt = booking.CreatedAt
            };

            // Set accommodation details based on type
            if (booking.LocalLodings.Any())
            {
                var localLoding = booking.LocalLodings.First();
                dto.AccommodationName = localLoding.AccommodationName;
                dto.AccommodationType = "Apartment";
            }
            else if (booking.HotelRooms.Any())
            {
                var hotelRoom = booking.HotelRooms.First();
                dto.AccommodationName = hotelRoom.Hotel.AccommodationName;
                dto.AccommodationType = "Hotel";
            }
            else if (booking.Beds.Any())
            {
                var bed = booking.Beds.First();
                dto.AccommodationName = bed.StudentRoom.StudentHouse.AccommodationName;
                dto.AccommodationType = "Student Housing";
            }

            return dto;
        }

        public async Task<AvailabilityResponseDto> GetAccommodationAvailabilityAsync(AccommodationAvailabilityDto availabilityDto)
        {
            var accommodation = await _context.Accommodations
                .FirstOrDefaultAsync(a => a.AccommodationID == availabilityDto.AccommodationID);

            if (accommodation == null)
                throw new ArgumentException("Accommodation not found");

            var startDate = new DateTime(availabilityDto.Year, availabilityDto.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var response = new AvailabilityResponseDto();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var isAvailable = await IsDateAvailableForAccommodationAsync(accommodation, date);

                if (isAvailable)
                    response.AvailableDates.Add(date);
                else
                    response.BookedDates.Add(date);
            }

            return response;
        }

        private async Task<bool> IsDateAvailableForAccommodationAsync(Accommodation accommodation, DateTime date)
        {
            return accommodation switch
            {
                Hotel hotel => await IsHotelDateAvailableAsync(hotel.AccommodationID, date),
                LocalLoding localLoding => await IsLocalLodingDateAvailableAsync(localLoding.AccommodationID, date),
                StudentHouse studentHouse => await IsStudentHouseDateAvailableAsync(studentHouse.AccommodationID, date),
                _ => false
            };
        }

        private async Task<bool> IsLocalLodingDateAvailableAsync(int localLodingId, DateTime date)
        {
            var localLoding = await _context.LocalLodings
                .Include(l => l.Bookings)
                .FirstOrDefaultAsync(l => l.AccommodationID == localLodingId);

            if (localLoding == null || !localLoding.IsAvailable)
                return false;

            var activeBookings = localLoding.Bookings?.Where(b => b.Status != "Cancelled").ToList()
                                ?? new List<Booking>();

            return !activeBookings.Any(b => date >= b.CheckIN && date < b.CheckOUT);
        }

        private async Task<bool> IsHotelDateAvailableAsync(int hotelId, DateTime date)
        {
            var availableRooms = await _context.HotelRooms
                .Where(hr => hr.AccommodationID == hotelId && hr.IsAvailable)
                .Include(hr => hr.Bookings)
                .ToListAsync();

            if (!availableRooms.Any())
                return false;

            foreach (var room in availableRooms)
            {
                var activeBookings = room.Bookings?.Where(b => b.Status != "Cancelled").ToList()
                                    ?? new List<Booking>();

                var isRoomAvailable = !activeBookings.Any(b => date >= b.CheckIN && date < b.CheckOUT);
                if (isRoomAvailable)
                    return true;
            }

            return false;
        }

        private async Task<bool> IsStudentHouseDateAvailableAsync(int studentHouseId, DateTime date)
        {
            var availableBeds = await _context.Beds
                .Include(b => b.StudentRoom)
                .Where(b => b.StudentRoom.StudentHouse.AccommodationID == studentHouseId && b.IsAvailable)
                .Include(b => b.Bookings)
                .ToListAsync();

            if (!availableBeds.Any())
                return false;

            foreach (var bed in availableBeds)
            {
                var activeBookings = bed.Bookings?.Where(b => b.Status != "Cancelled").ToList()
                                    ?? new List<Booking>();

                var isBedAvailable = !activeBookings.Any(b => date >= b.CheckIN && date < b.CheckOUT);
                if (isBedAvailable)
                    return true;
            }

            return false;
        }
        public async Task<List<DateTime>> GetAllAvailableDatesAsync(int accommodationId)
        {
            var accommodation = await _context.Accommodations
                .FirstOrDefaultAsync(a => a.AccommodationID == accommodationId);

            if (accommodation == null)
                throw new Exception("Accommodation not found");

            // جيب كل الحجوزات النشطة لهذا العقار
            var bookings = await _context.Bookings
                .Where(b => (b.LocalLodings.Any(ll => ll.AccommodationID == accommodationId) ||
                            b.HotelRooms.Any(hr => hr.AccommodationID == accommodationId) ||
                            b.Beds.Any(bed => bed.StudentRoom.StudentHouse.AccommodationID == accommodationId)) &&
                            b.Status != "Cancelled")
                .ToListAsync();

            var allAvailableDates = new List<DateTime>();
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddMonths(6); // لمدة 6 أشهر قادمة

            // loop through each day in the range
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var isBooked = bookings.Any(b => date >= b.CheckIN && date < b.CheckOUT);

                if (!isBooked)
                    allAvailableDates.Add(date);
            }

            return allAvailableDates;
        }

        public async Task<bool> UpdateBookingStatusAsync(int bookingId, UpdateBookingStatusDto dto)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingID == bookingId);

            if (booking == null)
                throw new ArgumentException("Booking not found");

            // BookingStatus Enum Check
            if (!Enum.TryParse<BookingStatus>(dto.Status, true, out var parsedStatus))
                throw new ArgumentException("Invalid booking status");

            booking.Status = parsedStatus.ToString();
            await _context.SaveChangesAsync();

            return true;
        }
    }
}