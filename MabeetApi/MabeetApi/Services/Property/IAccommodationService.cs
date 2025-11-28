using MabeetApi.DTOs.Property;

namespace MabeetApi.Interfaces
{
    public interface IAccommodationService
    {
        // CRUD Operations
        Task<AccommodationDetailDto> CreateAccommodationAsync(AccommodationCreateDto dto, string hostId);
        Task<IEnumerable<AccommodationListDto>> GetHostAccommodationsAsync(string hostId);
        Task<AccommodationDetailDto?> GetAccommodationByIdAsync(int id, string hostId);
        Task<AccommodationDetailDto?> UpdateAccommodationAsync(AccommodationUpdateDto dto, string hostId);
        Task<bool> DeleteAccommodationAsync(int id, string hostId);

        // Image Management
        Task<bool> UploadImagesAsync(ImageUploadDto dto, string hostId);
        Task<bool> SetMainImageAsync(int accommodationId, int imageId, string hostId);
        Task<bool> DeleteImageAsync(int accommodationId, int imageId, string hostId);

        // Amenities Management
        Task<bool> UpdateAmenitiesAsync(int accommodationId, List<int> amenityIds, string hostId);

        // Hotel Rooms Management
        Task<HotelRoomDto?> AddHotelRoomAsync(int accommodationId, HotelRoomCreateDto dto, string hostId);
        Task<bool> UpdateHotelRoomAsync(int roomId, HotelRoomCreateDto dto, string hostId);
        Task<bool> DeleteHotelRoomAsync(int roomId, string hostId);

        // Student Rooms Management
        Task<StudentRoomDto?> AddStudentRoomAsync(int accommodationId, StudentRoomCreateDto dto, string hostId);
        Task<bool> UpdateStudentRoomAsync(int roomId, StudentRoomUpdateDto dto, string hostId);
        Task<bool> DeleteStudentRoomAsync(int roomId, string hostId);

        // Beds Management
        Task<BedDto?> AddBedToRoomAsync(int roomId, BedCreateDto dto, string hostId);
        Task<bool> UpdateBedAsync(int bedId, BedCreateDto dto, string hostId);
        Task<bool> DeleteBedAsync(int bedId, string hostId);
    }
}