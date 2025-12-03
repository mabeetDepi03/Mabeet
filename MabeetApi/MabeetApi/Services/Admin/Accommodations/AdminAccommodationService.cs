using MabeetApi.Data;
using MabeetApi.DTOs.Admin.Accommodations;
using MabeetApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting; // 🛑 ضروري لحذف الملفات

namespace MabeetApi.Services.Admin.Accommodations
{
	public class AdminAccommodationService : IAdminAccommodationService
	{
		private readonly AppDbContext _context;
		private readonly IWebHostEnvironment _environment; // 🛑 إضافة البيئة

		public AdminAccommodationService(AppDbContext context, IWebHostEnvironment environment)
		{
			_context = context;
			_environment = environment;
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

		// delete accommodation (🛑 الدالة المعدلة)
		public async Task<bool> DeleteAsync(int id)
		{
			// 1. جلب العقار مع الصور
			var accommodation = await _context.Accommodations
				.Include(a => a.Images)
				.FirstOrDefaultAsync(a => a.AccommodationID == id);

			if (accommodation == null) return false;

			// 2. حذف الصور (من السيرفر والداتابيز)
			if (accommodation.Images != null && accommodation.Images.Any())
			{
				foreach (var img in accommodation.Images)
				{
					DeletePhysicalFile(img.ImageUrl);
				}
				_context.Images.RemoveRange(accommodation.Images);
			}

			// 3. حذف الغرف المرتبطة (لتجنب خطأ FK)
			if (accommodation is Hotel hotel)
			{
				// تحميل الغرف الفندقية
				await _context.Entry(hotel).Collection(h => h.HotelRooms).LoadAsync();

				if (hotel.HotelRooms != null && hotel.HotelRooms.Any())
				{
					_context.HotelRooms.RemoveRange(hotel.HotelRooms);
				}
			}
			else if (accommodation is StudentHouse studentHouse)
			{
				// تحميل الغرف الطلابية مع الأسرة
				await _context.Entry(studentHouse).Collection(sh => sh.StudentRooms)
					.Query().Include(r => r.Beds).LoadAsync();

				if (studentHouse.StudentRooms != null && studentHouse.StudentRooms.Any())
				{
					// حذف الأسرة أولاً
					var beds = studentHouse.StudentRooms.SelectMany(r => r.Beds).ToList();
					if (beds.Any())
						_context.Beds.RemoveRange(beds);

					// ثم حذف الغرف
					_context.StudentRooms.RemoveRange(studentHouse.StudentRooms);
				}
			}

			// 4. أخيراً حذف العقار نفسه
			_context.Accommodations.Remove(accommodation);
			await _context.SaveChangesAsync();
			return true;
		}

		// Helper: Delete File from wwwroot
		private void DeletePhysicalFile(string imageUrl)
		{
			if (!string.IsNullOrEmpty(imageUrl))
			{
				try
				{
					var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
					if (File.Exists(filePath))
					{
						File.Delete(filePath);
					}
				}
				catch
				{
					// تجاهل الأخطاء في حذف الملفات (المهم حذف السجل من الداتابيز)
				}
			}
		}
	}
}