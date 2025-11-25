using MabeetApi.Data;
using MabeetApi.DTOs.Admin.Accommodations;
using Microsoft.EntityFrameworkCore;

namespace MabeetApi.Services.Admin.Accommodations
{
    public class AdminAccommodationService : IAdminAccommodationService
    {
        private readonly AppDbContext _context;

        public AdminAccommodationService(AppDbContext context)
        {
            _context = context;
        }

        // get all accommodations
        public async Task<List<AdminAccommodationListDto>> GetAllAsync()
        {
            return await _context.Accommodations
                .Include(a => a.AppUser)
                .Include(a => a.Location)
                    .ThenInclude(l => l.City)
                .Select(a => new AdminAccommodationListDto
                {
                    AccommodationID = a.AccommodationID,
                    AccommodationName = a.AccommodationName,
                    AccommodationType = a.AccommodationType,
                    OwnerName = a.AppUser.FirstName + " " + a.AppUser.LastName,
                    CityName = a.Location.City.CityName,
                    IsApproved = a.IsApproved,
                    CreatedAt = a.CreatedAt
                })
                .ToListAsync();
        }

        // get accommodation by id
        public async Task<AdminAccommodationDetailsDto> GetByIdAsync(int id)
        {
            var accommodation = await _context.Accommodations
                .Include(a => a.AppUser)
                .Include(a => a.Location)
                    .ThenInclude(l => l.City)
                        .ThenInclude(c => c.Governorate)
                .Include(a => a.Images)
                .Include(a => a.Amenities)
                .FirstOrDefaultAsync(a => a.AccommodationID == id);

            if (accommodation == null) return null;

            return new AdminAccommodationDetailsDto
            {
                AccommodationID = accommodation.AccommodationID,
                AccommodationName = accommodation.AccommodationName,
                Description = accommodation.AccommodationDescription,
                AccommodationType = accommodation.AccommodationType,
                OwnerName = accommodation.AppUser.FirstName + " " + accommodation.AppUser.LastName,
                OwnerPhone = accommodation.AppUser.PhoneNumber,
                City = accommodation.Location.City.CityName,
                Governorate = accommodation.Location.City.Governorate.GovernorateName,
                Images = accommodation.Images.Select(i => i.ImageUrl).ToList(),
                Amenities = accommodation.Amenities.Select(m => m.AmenityName).ToList(),
                IsApproved = accommodation.IsApproved,
                CreatedAt = accommodation.CreatedAt
            };
        }

        // update accommodation status
        public async Task<bool> UpdateStatusAsync(int id, UpdateAccommodationStatusDto dto)
        {
            var accommodation = await _context.Accommodations.FindAsync(id);
            if (accommodation == null) return false;

            accommodation.IsApproved = dto.IsApproved;
            accommodation.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        // delete accommodation
        public async Task<bool> DeleteAsync(int id)
        {
            var accommodation = await _context.Accommodations.FindAsync(id);
            if (accommodation == null) return false;

            _context.Accommodations.Remove(accommodation);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
