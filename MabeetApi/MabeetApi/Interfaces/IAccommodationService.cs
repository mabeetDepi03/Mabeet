using MabeetApi.DTOs.Property;

namespace MabeetApi.Interfaces
{
	public interface IAccommodationService
	{
		// 1. Create
		Task<AccommodationDetailDto> CreateAccommodationAsync(AccommodationCreateDto dto, string hostId);

		// 2. Read
		Task<IEnumerable<AccommodationListDto>> GetHostAccommodationsAsync(string hostId);
		Task<AccommodationDetailDto?> GetAccommodationByIdAsync(int id, string hostId);

		// 3. Update
		Task<AccommodationDetailDto?> UpdateAccommodationAsync(AccommodationUpdateDto dto, string hostId);

		// 4. Delete
		Task<bool> DeleteAccommodationAsync(int id, string hostId);

		// 5. Images
		Task<bool> UploadImagesAsync(ImageUploadDto dto, string hostId);
		Task<bool> SetMainImageAsync(int accommodationId, int imageId, string hostId);
		Task<bool> DeleteImageAsync(int accommodationId, int imageId, string hostId);

		// 6. Amenities
		Task<bool> UpdateAmenitiesAsync(int accommodationId, List<int> amenityIds, string hostId);

		// 7. Hotel Rooms
		Task<HotelRoomDto?> AddHotelRoomAsync(int accommodationId, HotelRoomCreateDto dto, string hostId);
		Task<bool> UpdateHotelRoomAsync(int roomId, HotelRoomCreateDto dto, string hostId);
		Task<bool> DeleteHotelRoomAsync(int roomId, string hostId);

		// 8. Student Rooms & Beds
		Task<StudentRoomDto?> AddStudentRoomAsync(int accommodationId, StudentRoomCreateDto dto, string hostId);
		Task<bool> UpdateStudentRoomAsync(int roomId, StudentRoomUpdateDto dto, string hostId);
		Task<bool> DeleteStudentRoomAsync(int roomId, string hostId);

		// Beds
		Task<BedDto?> AddBedToRoomAsync(int roomId, BedCreateDto dto, string hostId);
		Task<bool> UpdateBedAsync(int bedId, BedCreateDto dto, string hostId);
		Task<bool> DeleteBedAsync(int bedId, string hostId);
	}
}
