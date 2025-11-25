using MabeetApi.DTOs.Admin.Accommodations;

namespace MabeetApi.Services.Admin.Accommodations
{
    public interface IAdminAccommodationService
    {
        Task<List<AdminAccommodationListDto>> GetAllAsync();
        Task<AdminAccommodationDetailsDto> GetByIdAsync(int id);
        Task<bool> UpdateStatusAsync(int id, UpdateAccommodationStatusDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
    