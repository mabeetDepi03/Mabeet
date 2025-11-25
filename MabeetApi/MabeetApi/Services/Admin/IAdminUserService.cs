using MabeetApi.DTOs.Admin;

namespace MabeetApi.Services.Admin
{
    public interface IAdminUserService
    {
        Task<List<AdminUserListDto>> GetAllUsersAsync();
        Task<AdminUserListDto> GetUserByIdAsync(string userId);
        Task<bool> ChangeUserRoleAsync(ChangeUserRoleDto dto);
        Task<bool> ToggleUserStatusAsync(ToggleUserStatusDto dto);
        Task<bool> DeleteUserAsync(string userId);
    }
}
